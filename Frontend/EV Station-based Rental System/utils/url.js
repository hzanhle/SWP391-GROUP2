// Resolve absolute/relative image URLs against Vehicle API base
export function resolveImageUrl(path) {
  const BASE_URL = (import.meta.env.VITE_VEHICLE_API_URL || '').replace(/\/$/, '')
  if (!path) return null
  if (/^https?:\/\//i.test(path)) return path
  return BASE_URL ? `${BASE_URL}/${String(path).replace(/^\//, '')}` : path
}
