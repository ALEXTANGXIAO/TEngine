local parse_core = require "core"
local buildin_types = parse_core.buildin_types

local gmatch = string.gmatch
local tsort = table.sort
local tconcat = table.concat
local sformat = string.format


local mt = {}
mt.__index = mt

local function upper_head(s)
  local c =  string.upper(string.sub(s, 1, 1))
  return c..string.sub(s, 2)
end

local function create_stream()
  return setmetatable({}, mt)
end

function mt:write(s, deep)
  s = s or ""
  deep = deep or 0

  local prefix = ""
  for i=1,deep do
    prefix = prefix.."\t"
  end

  self[#self+1] = prefix..s
end

function mt:dump()
  return tconcat( self, "\n")
end



local function str_split(str, sep)
  if sep == nil then
    sep = "%s"
  end

  local t={}
  local i=1
  for v in gmatch(str, "([^"..sep.."]+)") do
    t[i] = v
    i = i + 1
  end
  return t
end



-- class = {
--   {class_name = AAA, type_name = A.A.AAA,  max_field = 11, sproto_type = {}, internal_class = {}},
--   {class_name = BBB, type_name = B.B.BBB,  max_field = 11, sproto_type = {}, internal_class = {}},
-- }

local function _get_max_field_count(sproto_type)
  local maxn = #sproto_type
  local last_tag = -1

  for i=1,#sproto_type do
    local field = sproto_type[i]
    local tag = field.tag

    if tag < last_tag then
      error("tag must in ascending order")
    end

    if tag > last_tag +1 then 
      maxn = maxn + 1
    end
    last_tag = tag
  end

  return maxn
end

local function type2class(type_name, class_name, sproto_type)
  local class = {
    class_name = class_name,
    type_name = type_name, 
    max_field_count = sproto_type and _get_max_field_count(sproto_type) or nil,
    sproto_type = sproto_type,
    internal_class = {},
  }

  return class
end




local function gen_protocol_class(ast)
  local ret = {}
  for k,v in pairs(ast) do
    ret[#ret+1] = {
      name = k, 
      tag = v.tag,
      request = v.request,
      response = v.response,
    }
  end
  table.sort(ret, function (a, b) return a.name < b.name end)

  local cache = {}
  local classes = {}
  for i=1,#ret do
    local name = ret[i].name
    local fold = str_split(name, ".")

    local fullname = ""  
    local per = classes
    for i,v in ipairs(fold) do
      if i == 1 then fullname = v
      else fullname = fullname.."."..v end

      local item = cache[fullname]
      if not item then
        item = {}
        cache[fullname] = item
        table.insert(per, item)
      end

      per = item
      item.name = v
      item.value = ast[fullname]
    end
  end


  ret.classes = classes
  return ret
end



local function gen_type_class(ast)
  local type_name_list = {}
  local class = {}
  local cache = {}

  for k, _ in pairs(ast) do
    type_name_list[#type_name_list+1] = k
  end
  tsort(type_name_list, function (a, b) return a<b end)

  for i=1, #type_name_list do
    local k = type_name_list[i]
    local type_list = str_split(k, ".")
    
    local cur = class
    local type_name = ""
    for i=1,#type_list do
      local class_name = type_list[i]

      if i == 1 then type_name = class_name 
      else type_name = type_name.."."..class_name end

      if not cache[type_name] then
        local sproto_type = ast[type_name]
        local class_info =  type2class(sproto_type and type_name or nil, class_name, sproto_type)
        cur[#cur+1] = class_info
        cache[type_name] = class_info
      end

      cur = cache[type_name].internal_class
    end

  end

  return class
end

local _class_type = {
  string = "string",
  integer = "Int64",
  boolean = "bool",
  binary = "byte[]",
  double = "double",
}
local function _typename2internal(field)
  local t = field.typename
  local decimal = field.decimal
  if t == "integer" and decimal then
    t = "double"
  else
    t = _class_type[t] or t
  end
  return t
end

local function _2class_type(field)
  local t = _typename2internal(field)
  local is_array = field.array
  local key = field.key
  local decimal = field.decimal


  if is_array and key then -- map
    local tk = _typename2internal(field.map_keyfield)
    assert(tk, "Invalid map key.")
    return string.format("Dictionary<%s, %s>", tk, t)
  elseif is_array and not key then -- array
    return "List<"..t..">"
  elseif not is_array and not key then -- element
    return t
  else
    error("Invalid field type.")
  end
end


local _write_func = {
  string = "write_string",
  integer = "write_integer",
  boolean = "write_boolean",
  binary = "write_binary",
  double = "write_double",
}
local function _write_encode_field(field, idx, stream, deep)
  local typename = field.typename
  local tag = field.tag
  local name = field.name
  local decimal = field.decimal
  local func_name = nil

  stream:write(sformat("if (base.has_field.has_field (%d)) {", idx), deep)
  if typename == "integer" and decimal then
    decimal = 10^decimal
    stream:write(sformat("base.serialize.write_decimal (this.%s, %s, %d);", name, decimal, tag), deep+1)
  else
    func_name = _write_func[typename] or "write_obj"
    stream:write(sformat("base.serialize.%s (this.%s, %d);", func_name, name, tag), deep+1)
  end
  stream:write("}\n", deep)
end


local _read_func = {
  string = "read_string",
  integer = "read_integer",
  boolean = "read_boolean",
  binary = "read_binary",
  double = "read_double",
}
local function _write_read_field(field, stream, deep)
  local typename = field.typename
  local is_array = field.array
  local tag = field.tag
  local name = field.name
  local key = field.key
  local decimal = field.decimal

  local func_name= nil
  if typename == "integer" and decimal then
    decimal = 10^decimal
    func_name ="read_decimal"
  else
    func_name = _read_func[typename]
  end

  stream:write("case "..(tag)..":", deep)
  if func_name then
    if is_array then func_name = func_name.."_list" end
    local s = string.format("this."..name.." = base.deserialize."..func_name.." (%s);",
      decimal or "")
    stream:write(s, deep+1)

  elseif key then
    assert(is_array)
    local map_keyfield = field.map_keyfield
    local fmt = string.format("this.%s = base.deserialize.read_map<%s, %s>(v => v.%s);",
      name,
      _typename2internal(map_keyfield), typename,
      map_keyfield.name)
    stream:write(fmt, deep+1)

  else
    func_name = "read_obj"
    if is_array then func_name = func_name.."_list" end
    stream:write("this."..name.." = base.deserialize."..func_name.."<"..typename..">".." ();", deep+1)

  end
  stream:write("break;", deep+1)
end



local function dump_class(class_info, stream, deep)
  local class_name = class_info.class_name
  local sproto_type = class_info.sproto_type
  local internal_class = class_info.internal_class
  local max_field_count = class_info.max_field_count

  if sproto_type then
    stream:write("public class "..class_name.." : SprotoTypeBase {", deep)
    
    -- max_field_count
    deep = deep + 1;
    stream:write("private static int max_field_count = "..(max_field_count)..";", deep)

    -- internal class
    stream:write("", deep)
    for i=1,#internal_class do
      dump_class(internal_class[i], stream, deep)
    end

    -- property
    stream:write("", deep)
    for i=1,#sproto_type do
      local field = sproto_type[i]
      local type = _2class_type(field)
      local name = field.name
      local tag = field.tag

      stream:write(sformat("private %s _%s; // tag %d", type, name, tag), deep)
      stream:write(sformat("public %s %s {", type, name), deep)
        stream:write(sformat("get { return _%s; }", name), deep+1)
        stream:write(sformat("set { base.has_field.set_field (%d, true); _%s = value; }", i-1, name), deep+1)
      stream:write("}", deep)

      stream:write("public bool Has"..upper_head(name).." {", deep)
        stream:write(sformat("get { return base.has_field.has_field (%d); }", i-1), deep+1)
      stream:write("}\n", deep)
    end

    -- default constructor function
    stream:write("public "..class_name.." () : base(max_field_count) {}\n", deep)


    -- constructor function
    stream:write("public "..class_name.." (byte[] buffer) : base(max_field_count, buffer) {", deep)
      stream:write("this.decode ();", deep+1)
    stream:write("}\n", deep)


    -- decode function
    stream:write("protected override void decode () {", deep)
      stream:write("int tag = -1;", deep+1)
      stream:write("while (-1 != (tag = base.deserialize.read_tag ())) {", deep+1)
        stream:write("switch (tag) {", deep+2)
        for i=1,#sproto_type do
          local field = sproto_type[i]
          _write_read_field(field, stream, deep+2)
        end
        stream:write("default:", deep+2)
          stream:write("base.deserialize.read_unknow_data ();", deep+3)
          stream:write("break;", deep+3)
        stream:write("}", deep+2)
      stream:write("}", deep+1)
    stream:write("}\n", deep)


    -- encode function 
    stream:write("public override int encode (SprotoStream stream) {", deep)
      stream:write("base.serialize.open (stream);\n", deep+1)
      for i=1,#sproto_type do
        local field = sproto_type[i]
        _write_encode_field(field, i-1, stream, deep+1)
      end
      stream:write("return base.serialize.close ();", deep+1);
    stream:write("}", deep)


    deep = deep - 1;
    stream:write("}\n\n", deep)

  else
    stream:write("public class "..class_name.." {", deep)

    -- internal class
    stream:write("", deep)
    for i=1,#internal_class do
      dump_class(internal_class[i], stream, deep+1)
    end
    stream:write("}\n\n", deep)
  end
end


local function _gen_sprototype_namespace(package)
  return upper_head(package).."SprotoType"
end


local function _gen_protocol_classname(package)
  return upper_head(package).."Protocol"
end

local function constructor_protocol(class, package, stream, deep)
  local class_name = _gen_protocol_classname(package)
  local type_namespace = _gen_sprototype_namespace(package)

  stream:write("private "..class_name.."() {", deep)
  deep = deep + 1
    for _,class_info in ipairs(class) do
      local name = class_info.name
      local tag = class_info.tag
      local request_type = class_info.request
      local response_type = class_info.response
      local stag = name..".Tag"

      stream:write("Protocol.SetProtocol<"..name.."> ("..stag..");", deep)
      
      if request_type then
        request_type = type_namespace.."."..request_type
        stream:write("Protocol.SetRequest<"..request_type.."> ("..stag..");",deep)
      end

      if response_type then
        response_type = type_namespace.."."..response_type
        stream:write("Protocol.SetResponse<"..response_type.."> ("..stag..");", deep)
      end
      stream:write()
    end
  deep = deep - 1
  stream:write("}\n", deep)
end


local function dump_protocol_class(class, stream, deep)
  local name = class.name
  local value = class.value

  stream:write("public class "..name.." {", deep)
    if value then
      assert(#class == 0)
      stream:write("public const int Tag = "..value.tag..";", deep+1)
    else
      for i,v in ipairs(class) do
        dump_protocol_class(v, stream, deep+1)
      end
    end
  stream:write("}\n", deep)
end



local function parse_protocol(class, stream, package)
  if not class or #class == 0 then return end

  local class_name = _gen_protocol_classname(package)
  stream:write("public class "..class_name.." : ProtocolBase {")
    stream:write("public static  "..class_name.." Instance = new "..class_name.."();", 1)
    constructor_protocol(class, package, stream, 1)

    for i,v in ipairs(class.classes) do
      dump_protocol_class(v, stream, 1)
    end
  stream:write("}")
end


local function parse_type(class, stream, package)
  if not class or #class == 0 then return end

  local namespace = _gen_sprototype_namespace(package)
  stream:write("namespace "..namespace.." { ")

  for i=1,#class do
    local class_info = class[i]
    dump_class(class_info, stream, 1)
  end

  stream:write("}\n\n")
end

local header = [[// Generated by sprotodump. DO NOT EDIT!]]
local using = [[
using System;
using Sproto;
using System.Collections.Generic;
]]


local function parse_ast2type(ast, package, name)
  package = package or ""
  local type_class = gen_type_class(ast)
  local stream = create_stream()

  stream:write(header)
  stream:write([[// source: ]]..(name or "input").."\n")
  stream:write(using)

  -- parse type
  parse_type(type_class, stream, package)

  return stream:dump()
end


local function parse_ast2protocol(ast, package)
  package = package or ""
  local protocol_class = gen_protocol_class(ast)
  local stream = create_stream()

  stream:write(header)
  stream:write(using)

  -- parse protocol
  parse_protocol(protocol_class, stream, package)

  return stream:dump()  
end


local function parse_ast2all(ast, package, name)
  package = package or ""
  local type_class = gen_type_class(ast.type)
  local protocol_class = gen_protocol_class(ast.protocol)
  local stream = create_stream()

  stream:write(header)
  stream:write([[// source: ]]..(name or "input").."\n")
  stream:write(using)

  parse_type(type_class, stream, package)
  parse_protocol(protocol_class, stream, package)

  return stream:dump()  
end



------------------------------- dump -------------------------------------
local util = require "util"

local function main(trunk, build, param)
  local package = util.path_basename(param.package or "")
  local outfile = param.outfile
  local dir = param.dircetory or ""

  if outfile then
    local data = parse_ast2all(build, package, table.concat(param.sproto_file, " "))
    util.write_file(dir..outfile, data, "w")
  else
    -- dump sprototype
    for i,v in ipairs(trunk) do
      local name = param.sproto_file[i]
      local outcs = util.path_basename(name)..".cs"
      local data = parse_ast2type(v.type, package, name)
      util.write_file(dir..outcs, data, "w")
    end

    -- dump protocol
    if build.protocol then
      local data = parse_ast2protocol(build.protocol, package)
      local outfile = package.."Protocol.cs"
      util.write_file(dir..outfile, data, "w")
    end
  end
end

return main
