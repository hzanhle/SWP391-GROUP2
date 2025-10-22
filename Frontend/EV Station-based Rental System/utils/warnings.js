const KEY = 'admin.warned.users'

function read() {
  try {
    const raw = localStorage.getItem(KEY)
    return raw ? JSON.parse(raw) : {}
  } catch {
    return {}
  }
}

function write(map) {
  try { localStorage.setItem(KEY, JSON.stringify(map)) } catch {}
}

export function isWarned(userId) {
  const map = read()
  return !!map[String(userId)]
}

export function setWarned(userId, warned) {
  const map = read()
  if (warned) map[String(userId)] = true
  else delete map[String(userId)]
  write(map)
}

export function getWarnedMap() {
  return read()
}
