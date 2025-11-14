export function highlightText(text, searchQuery) {
  if (!searchQuery || !text) return text
  
  const regex = new RegExp(`(${searchQuery.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi')
  const parts = String(text).split(regex)
  
  return parts.map((part, index) => 
    regex.test(part) ? (
      <mark key={index} style={{ backgroundColor: '#ffeb3b', padding: '0 2px', borderRadius: '2px' }}>
        {part}
      </mark>
    ) : part
  )
}

