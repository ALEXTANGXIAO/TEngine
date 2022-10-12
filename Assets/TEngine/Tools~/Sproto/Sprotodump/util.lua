local M = {}

function M.path_basename(string_)
  local LUA_DIRSEP = string.sub(package.config,1,1)
  string_ = string_ or ''
  local basename = string.gsub (string_, '[^'.. LUA_DIRSEP ..']*'.. LUA_DIRSEP ..'', '')
  basename = string.gsub(basename, "(.+)%..+$", "%1")
  return basename
end


function M.file_basename(path)
  local file = string.gsub(path, "^.*[/\\](.+)$", "%1")
  local name = string.gsub(file, "^(.+)%..+$", "%1")
  return name
end


function M.read_file(path)
  local handle = io.open(path, "r")
  local ret = handle:read("*a")
  handle:close()
  return ret
end

function M.write_file(path, data, mode)
  local handle = io.open(path, mode)
  handle:write(data)
  handle:close()
  print("dump to "..path.." file")
end


return M