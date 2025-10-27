import React, { useId, useMemo, useRef, useState } from 'react'

function isImage(file) {
  return file && /^image\//.test(file.type);
}

export default function DocumentUploader({ label, hint, accept = 'image/*,application/pdf', value, onChange, required = false }) {
  const inputId = useId()
  const fileInputRef = useRef(null)
  const [internalFile, setInternalFile] = useState(null)

  const file = value ?? internalFile

  const previewUrl = useMemo(() => {
    if (!file) return ''
    if (isImage(file)) return URL.createObjectURL(file)
    return ''
  }, [file])

  function handlePick() {
    fileInputRef.current?.click()
  }

  function handleFile(e) {
    const f = e.target.files && e.target.files[0]
    if (!f) return
    if (onChange) onChange(f)
    else setInternalFile(f)
  }

  function clearFile() {
    if (onChange) onChange(null)
    else setInternalFile(null)
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  return (
    <div className="uploader">
      <label htmlFor={inputId} className="label">{label}{required ? ' *' : ''}</label>
      <input id={inputId} ref={fileInputRef} className="sr-only" type="file" accept={accept} onChange={handleFile} aria-label={label} />

      <div className="uploader-dropzone" role="button" tabIndex={0} onClick={handlePick} onKeyDown={(e)=>{ if(e.key==='Enter'||e.key===' ') handlePick() }} aria-describedby={`${inputId}-hint`}>
        {file ? (
          isImage(file) ? (
            <img src={previewUrl} alt={`${label} preview`} className="uploader-preview" />
          ) : (
            <div className="uploader-file">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="#64748b" strokeWidth="2"/><path d="M14 2v6h6" stroke="#64748b" strokeWidth="2"/></svg>
              <span className="uploader-filename">{file.name}</span>
            </div>
          )
        ) : (
          <div className="uploader-placeholder">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 5v14M5 12h14" stroke="#0ea5e9" strokeWidth="2" strokeLinecap="round"/></svg>
            <span>Chọn tệp để tải lên</span>
          </div>
        )}
      </div>
      {hint ? <p id={`${inputId}-hint`} className="uploader-hint">{hint}</p> : null}
      <div className="uploader-actions">
        <button type="button" className="btn btn-secondary" onClick={handlePick}>Chọn tệp</button>
        {file ? <button type="button" className="btn btn-ghost" onClick={clearFile}>Xoá</button> : null}
      </div>
    </div>
  )
}
