local parse_core = require "core"
local buildin_types = parse_core.buildin_types

local packbytes
local packvalue

if _VERSION == "Lua 5.3" then
  function packbytes(str)
    return string.pack("<s4",str)
  end

  function packvalue(id)
    id = (id + 1) * 2
    return string.pack("<I2",id)
  end
else
  function packbytes(str)
    local size = #str
    local a = size % 256
    size = math.floor(size / 256)
    local b = size % 256
    size = math.floor(size / 256)
    local c = size % 256
    size = math.floor(size / 256)
    local d = size
    return string.char(a)..string.char(b)..string.char(c)..string.char(d) .. str
  end

  function packvalue(id)
    id = (id + 1) * 2
    assert(id >=0 and id < 65536)
    local a = id % 256
    local b = math.floor(id / 256)
    return string.char(a) .. string.char(b)
  end
end


--[[
-- The protocol of sproto
.type {
  .field {
    name 0 : string
    buildin 1 : integer
    type 2 : integer
    tag 3 : integer
    array 4 : boolean
    key 5 : integer # If key exists, array must be true
    map 6 : boolean # Interpreted two fields struct as map when decoding
  }
  name 0 : string
  fields 1 : *field
}

.protocol {
  name 0 : string
  tag 1 : integer
  request 2 : integer # index
  response 3 : integer # index
}

.group {
  type 0 : *type
  protocol 1 : *protocol
}
]]

local function packfield(f)
  local strtbl = {}
  if f.array then
    if f.key then
      if f.map then
        table.insert(strtbl, "\7\0")  -- 7 fields
      else
        table.insert(strtbl, "\6\0")  -- 6 fields
      end
    else
      table.insert(strtbl, "\5\0")  -- 5 fields
    end
  else
    table.insert(strtbl, "\4\0")  -- 4 fields
  end
  table.insert(strtbl, "\0\0")  -- name (tag = 0, ref an object)
  if f.buildin then
    table.insert(strtbl, packvalue(f.buildin))  -- buildin (tag = 1)
    if f.extra then
        table.insert(strtbl, packvalue(f.extra))  -- f.buildin can be integer or string
    else
        table.insert(strtbl, "\1\0")          -- skip (tag = 2)
    end
    table.insert(strtbl, packvalue(f.tag))    -- tag (tag = 3)
  else
    table.insert(strtbl, "\1\0")  -- skip (tag = 1)
    table.insert(strtbl, packvalue(f.type))   -- type (tag = 2)
    table.insert(strtbl, packvalue(f.tag))    -- tag (tag = 3)
  end
  if f.array then
    table.insert(strtbl, packvalue(1))  -- array = true (tag = 4)
    if f.key then
      table.insert(strtbl, packvalue(f.key)) -- key tag (tag = 5)
      if f.map then
        table.insert(strtbl, packvalue(f.map)) -- map tag (tag = 6)
      end
    end
  end
  table.insert(strtbl, packbytes(f.name)) -- external object (name)
  return packbytes(table.concat(strtbl))
end

local function packtype(name, t, alltypes)
  local fields = {}
  local tmp = {}
  for _, f in ipairs(t) do
    tmp.array = f.array
    tmp.name = f.name
    tmp.tag = f.tag
    tmp.extra = f.decimal

    tmp.buildin = buildin_types[f.typename]
    if f.typename == "binary" then
      tmp.extra = 1  -- binary is sub type of string
    end
    local subtype
    if not tmp.buildin then
      subtype = assert(alltypes[f.typename])
      tmp.type = subtype.id
    else
      tmp.type = nil
    end
    tmp.map = nil
    if f.key then
      assert(f.array)
      if f.key == "" then
        tmp.map = 1
        local c = 0
        local min_t = math.maxinteger
        for n, t in pairs(subtype.fields) do
          c = c + 1
          if t.tag < min_t then
            min_t = t.tag
            f.key = n
          end
        end
        if c ~= 2 then
          error(string.format("Invalid map definition: %s, must only have two fields", tmp.name))
        end
      end
      local stfield = subtype.fields[f.key]
      if not stfield or not stfield.buildin then
        error("Invalid map index :" .. f.key)
      end
      tmp.key = stfield.tag
    else
      tmp.key = nil
    end

    table.insert(fields, packfield(tmp))
  end
  local data
  if #fields == 0 then
    data = {
      "\1\0",  -- 1 fields
      "\0\0",  -- name  (id = 0, ref = 0)
      packbytes(name),
    }
  else
    data = {
      "\2\0",  -- 2 fields
      "\0\0",  -- name  (tag = 0, ref = 0)
      "\0\0", -- field[]  (tag = 1, ref = 1)
      packbytes(name),
      packbytes(table.concat(fields)),
    }
  end

  return packbytes(table.concat(data))
end

local function packproto(name, p, alltypes)
  if p.request then
    local request = alltypes[p.request]
    if request == nil then
      error(string.format("Protocol %s request type %s not found", name, p.request))
    end
    request = request.id
  end
  local tmp = {
    "\4\0",	-- 4 fields
    "\0\0",  -- name (id=0, ref=0)
    packvalue(p.tag),  -- tag (tag=1)
  }
  if p.request == nil and p.response == nil and p.confirm == nil then
    tmp[1] = "\2\0"  -- only two fields
  else
    if p.request then
      table.insert(tmp, packvalue(alltypes[p.request].id)) -- request typename (tag=2)
    else
      table.insert(tmp, "\1\0")  -- skip this field (request)
    end
    if p.response then
      table.insert(tmp, packvalue(alltypes[p.response].id)) -- request typename (tag=3)
    elseif p.confirm then
      tmp[1] = "\5\0"  -- add confirm field
      table.insert(tmp, "\1\0")  -- skip this field (response)
      table.insert(tmp, packvalue(1))  -- confirm = true
    else
      tmp[1] = "\3\0"  -- only three fields
    end
  end

  table.insert(tmp, packbytes(name))

  return packbytes(table.concat(tmp))
end

local function packgroup(t,p)
  if next(t) == nil then
    assert(next(p) == nil)
    return "\0\0"
  end
  local tt, tp
  local alltypes = {}
  for name in pairs(t) do
    table.insert(alltypes, name)
  end
  table.sort(alltypes)  -- make result stable
  for idx, name in ipairs(alltypes) do
    local fields = {}
    for _, type_fields in ipairs(t[name]) do
      fields[type_fields.name] = {
        tag = type_fields.tag,
        buildin = buildin_types[type_fields.typename]
      }
    end
    alltypes[name] = { id = idx - 1, fields = fields }
  end
  tt = {}
  for _,name in ipairs(alltypes) do
    table.insert(tt, packtype(name, t[name], alltypes))
  end
  tt = packbytes(table.concat(tt))
  if next(p) then
    local tmp = {}
    for name, tbl in pairs(p) do
      table.insert(tmp, tbl)
      tbl.name = name
    end
    table.sort(tmp, function(a,b) return a.tag < b.tag end)

    tp = {}
    for _, tbl in ipairs(tmp) do
      table.insert(tp, packproto(tbl.name, tbl, alltypes))
    end
    tp = packbytes(table.concat(tp))
  end
  local result
  if tp == nil then
    result = {
      "\1\0", -- 1 field
      "\0\0", -- type[] (id = 0, ref = 0)
      tt,
    }
  else
    result = {
      "\2\0", -- 2fields
      "\0\0", -- type array (id = 0, ref = 0)
      "\0\0", -- protocol array (id = 1, ref =1)

      tt,
      tp,
    }
  end

  return table.concat(result)
end

local function encodeall(r)
  return packgroup(r.type, r.protocol)
end

local function dump(str)
  local tmp = ""
  for i=1,#str do
    tmp = tmp .. string.format("%02X ", string.byte(str,i))
    if i % 8 == 0 then
      if i % 16 == 0 then
        print(tmp)
        tmp = ""
      else
        tmp = tmp .. "- "
      end
    end
  end
  print(tmp)
end


local function parse_ast(ast)
  return encodeall(ast)
end

------------------------------- dump -------------------------------------
local util = require "util"

local function main(trunk, build, param)
  local outfile = param.outfile or param.package and util.path_basename(param.package)..".spb" or "sproto.spb"
  outfile = (param.dircetory or "")..outfile
  local data = parse_ast(build)
  util.write_file(outfile, data, "wb")
end


return main
