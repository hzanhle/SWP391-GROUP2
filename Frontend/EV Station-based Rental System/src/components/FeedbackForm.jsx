import React, { useState } from 'react'
import CTA from './CTA'

export default function FeedbackForm({ open, onClose, onSubmit, defaultRating = 5 }) {
  const [rating, setRating] = useState(defaultRating)
  const [comments, setComments] = useState('')
  const [submitting, setSubmitting] = useState(false)

  if (!open) return null

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      setSubmitting(true)
      await onSubmit({ rating, comments })
    } catch (err) {
      // swallow - parent will show errors
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop">
      <div className="modal" role="dialog" aria-modal="true">
        <div className="modal-header">
          <h3>Đánh giá trải nghiệm</h3>
        </div>
        <form className="modal-body" onSubmit={handleSubmit}>
          <label className="field">
            <span className="label">Đánh giá xe (1-5)</span>
            <select value={rating} onChange={(e) => setRating(Number(e.target.value))} className="input">
              {[5,4,3,2,1].map(n => <option key={n} value={n}>{n} sao</option>)}
            </select>
          </label>

          <label className="field">
            <span className="label">Bình luận (tuỳ chọn)</span>
            <textarea className="input" value={comments} onChange={(e) => setComments(e.target.value)} rows={4} />
          </label>

          <div className="row-between" style={{ marginTop: '1rem' }}>
            <CTA as="button" type="submit" variant="primary" disabled={submitting}>
              {submitting ? 'Đang gửi...' : 'Gửi đánh giá'}
            </CTA>
            <CTA as="button" variant="ghost" onClick={onClose} type="button">Đóng</CTA>
          </div>
        </form>
      </div>
    </div>
  )
}
