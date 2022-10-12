------------- stream -------------
local stream_mt = {}
local function new_stream(...)
  local raw = {...}
  raw.count = #raw
  raw.cur_idx = 1
  return setmetatable(raw, {__index = stream_mt})
end


function stream_mt:read()
  local len = self.count
  if self.cur_idx <= len then
    local ret = self[self.cur_idx]
    self.cur_idx = self.cur_idx + 1
    return ret
  end
end


function stream_mt:read_value()
  if not self:is_opt() then
    return self:read()
  end
end


function stream_mt:is_opt()
  local cur_v = self[self.cur_idx]
  if cur_v then
    return string.match(cur_v, "^%-.+$")
  end
end


function stream_mt:current()
  return self[self.cur_idx]
end

function stream_mt:is_end()
  return self.cur_idx > #self
end

------------- parser -------------
local function _parser_opt(stream, n)
  if stream:is_opt() then
    if n then
      for i=1,n do
        if not stream[stream.cur_idx+i] then
          return false
        end
      end
    end

    local ret = {
      opt = stream:read(),
    }

    if not n then
      while true do
        local v = stream:read_value()
        if not v then break end
        table.insert(ret, v)
      end
    else
      for i=1,n do
        local v = stream:read_value()
        if not v then break end
        table.insert(ret, v)
      end
    end
    return ret
  end
end


local function parser_opt(stream)
  return _parser_opt(stream)
end


------------- param -------------
local function parse_param(...)
  local stream = new_stream(...)

  if #stream == 0 then
    return false
  end

  local ret = {
    dircetory = false,
    package = false,
    outfile = false,
    namespace = false,

    sproto_file = {},
    dump_type = false,
  }


  --- parser option
  local result = parser_opt(stream)
  if not result then
    return false
  else
    ret.dump_type = result.opt
    for i,v in ipairs(result) do
      ret.sproto_file[i] = v
    end
  end

  --- parser out option
  local out_option = {
    ["-d"] = { "dircetory", 1},
    ["-o"] = { "outfile", 1},
    ["-p"] = { "package", 1},
    ["-split"] = { "split", 1},
    ["-crypt"] = { "crypt", 0},
    ["-namespace"] = { "namespace", 0},
  }
  while not stream:is_end() do
    local k = stream:current()
    local cfg = out_option[k]
    if not cfg then
      error("invalid option:%s"..tostring(k))
    end
    local name = cfg[1]
    local vcount = cfg[2]
    local result = _parser_opt(stream, vcount)
    if not result then
      local s = string.format("parser param %s(%s) is error", k, name)
      error(s)
    end
    if vcount == 1 then
      local v = result[1]
      if not v then
        local s = string.format("parser param %s(%s) is error, value is needed", k, name)
        error(s) 
      end
      ret[name] = v
    elseif vcount  == 0 then
      ret[name] = true
    elseif vcount > 0 then
      local entry = {}
      for i,v in ipairs(result) do
        entry[i] = v
      end
      ret[name] = entry
    else
      assert(false)
    end
  end
  
  return ret
end

return parse_param


