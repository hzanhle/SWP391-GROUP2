import React, { useEffect, useMemo, useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import { getAllModels, createModel, updateModel, deleteModel } from '../api/vehicle'

function ModelRow({ m, onEdit, onDelete }) {
  return (
    <div className="card">
      <div className="card-body">
        <div className="row-between">
          <div>
            <div className="card-title">{m.modelName} • {m.manufacturer} ({m.year})</div>
            <div className="card-subtext">Tốc độ: {m.maxSpeed} km/h • Pin: {m.batteryCapacity} mAh • Tầm: {m.batteryRange} km • Chỗ: {m.vehicleCapacity}</div>
            <div className="card-subtext">Giá mẫu: {m.modelCost?.toLocaleString?.() ?? m.modelCost} • Giá/giờ: {m.rentFeeForHour?.toLocaleString?.() ?? m.rentFeeForHour}</div>
          </div>
          <div className="row">
            <CTA as="button" onClick={() => onEdit(m)}>Sửa</CTA>
            <button className="btn" onClick={() => onDelete(m)}>Xóa</button>
          </div>
        </div>
      </div>
    </div>
  )
}

function ModelForm({ initial, onSubmit, onCancel }) {
  const [form, setForm] = useState(() => ({
    modelName: initial?.modelName || '',
    manufacturer: initial?.manufacturer || '',
    year: initial?.year || 2025,
    maxSpeed: initial?.maxSpeed || 60,
    batteryCapacity: initial?.batteryCapacity || 5000,
    chargingTime: initial?.chargingTime || 120,
    batteryRange: initial?.batteryRange || 100,
    vehicleCapacity: initial?.vehicleCapacity || 2,
    modelCost: initial?.modelCost || 10000000,
    rentFeeForHour: initial?.rentFeeForHour || 50000,
    files: [],
  }))

  function updateField(k, v) { setForm(prev => ({ ...prev, [k]: v })) }

  function handleFiles(e) {
    updateField('files', Array.from(e.target.files || []))
  }

  async function submit(e) {
    e.preventDefault()
    await onSubmit(form)
  }

  return (
    <form className="card-body" onSubmit={submit} noValidate>
      <div className="docs-grid">
        <div className="field"><label className="label">T��n mẫu</label><input className="input" value={form.modelName} onChange={e=>updateField('modelName', e.target.value)} required /></div>
        <div className="field"><label className="label">Hãng</label><input className="input" value={form.manufacturer} onChange={e=>updateField('manufacturer', e.target.value)} required /></div>
        <div className="field"><label className="label">Năm</label><input className="input" type="number" value={form.year} onChange={e=>updateField('year', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Tốc độ tối đa (km/h)</label><input className="input" type="number" value={form.maxSpeed} onChange={e=>updateField('maxSpeed', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Dung lượng pin (mAh)</label><input className="input" type="number" value={form.batteryCapacity} onChange={e=>updateField('batteryCapacity', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Thời gian sạc (phút)</label><input className="input" type="number" value={form.chargingTime} onChange={e=>updateField('chargingTime', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Quãng đường (km)</label><input className="input" type="number" value={form.batteryRange} onChange={e=>updateField('batteryRange', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Số chỗ</label><input className="input" type="number" value={form.vehicleCapacity} onChange={e=>updateField('vehicleCapacity', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Giá mẫu (VNĐ)</label><input className="input" type="number" value={form.modelCost} onChange={e=>updateField('modelCost', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Giá thuê/giờ (VNĐ)</label><input className="input" type="number" value={form.rentFeeForHour} onChange={e=>updateField('rentFeeForHour', Number(e.target.value))} required /></div>
        <div className="field"><label className="label">Hình ảnh</label><input className="input" type="file" multiple onChange={handleFiles} /></div>
      </div>
      <div className="row-between">
        <button type="button" className="btn" onClick={onCancel}>Hủy</button>
        <CTA as="button" type="submit">Lưu</CTA>
      </div>
    </form>
  )
}

export default function AdminModels() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [models, setModels] = useState([])
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [editing, setEditing] = useState(null)
  const token = typeof window !== 'undefined' ? localStorage.getItem('auth.token') : null

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    const list = Array.isArray(models) ? models : []
    const f = q ? list.filter(m => (m.modelName||'').toLowerCase().includes(q) || (m.manufacturer||'').toLowerCase().includes(q)) : list
    return f
  }, [models, search])

  const total = filtered.length
  const start = (page - 1) * pageSize
  const visible = filtered.slice(start, start + pageSize)

  async function load() {
    setLoading(true)
    setError('')
    try {
      const { data } = await getAllModels(token)
      setModels(Array.isArray(data) ? data : [])
    } catch (e) {
      setError(e?.message || 'Không tải được danh sách mẫu')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleCreate() {
    setEditing({})
  }

  async function handleEdit(m) {
    setEditing(m)
  }

  async function handleDelete(m) {
    if (!confirm('Xóa mẫu này?')) return
    try {
      setLoading(true)
      await deleteModel(m.modelId, token)
      await load()
    } catch (e) {
      alert(e?.message || 'Xóa thất bại')
    } finally {
      setLoading(false)
    }
  }

  async function handleSubmit(form) {
    try {
      setLoading(true)
      if (editing && editing.modelId) {
        await updateModel(editing.modelId, form, token)
      } else {
        await createModel(form, token)
      }
      setEditing(null)
      await load()
    } catch (e) {
      alert(e?.message || 'Lưu thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div data-figma-layer="Admin Models">
      <Navbar />
      <main>
        <section className="section" aria-labelledby="admin-models-title">
          <div className="container">
            <div className="section-header">
              <h1 id="admin-models-title" className="section-title">Quản lý mẫu xe</h1>
              <p className="section-subtitle">Thêm, sửa, xóa mẫu xe hai bánh.</p>
            </div>

            <div className="card">
              <div className="card-body">
                {error ? <div role="alert" className="badge gray">{error}</div> : null}
                <div className="row-between">
                  <div className="field" style={{minWidth: '260px'}}>
                    <label className="label">Tìm kiếm</label>
                    <input className="input" value={search} onChange={e=>{ setSearch(e.target.value); setPage(1); }} placeholder="Tên mẫu, hãng" />
                  </div>
                  <CTA as="button" onClick={handleCreate}>Thêm mẫu</CTA>
                </div>
              </div>
            </div>

            {loading && (
              <div className="text-center py-10"><div className="spinner" /></div>
            )}

            {!loading && (
              <div className="docs-grid">
                {visible.map(m => (
                  <ModelRow key={m.modelId} m={m} onEdit={handleEdit} onDelete={handleDelete} />
                ))}
              </div>
            )}

            <div className="row-between">
              <button className="btn" disabled={page<=1} onClick={()=>setPage(p=>Math.max(1,p-1))}>Trước</button>
              <div className="card-subtext">Trang {page} / {Math.max(1, Math.ceil(total / pageSize))} (Tổng {total})</div>
              <button className="btn" disabled={page>=Math.ceil(total/pageSize)} onClick={()=>setPage(p=>p+1)}>Sau</button>
            </div>

            {editing !== null && (
              <div className="card">
                <div className="card-header"><h2 className="card-title">{editing?.modelId ? 'Sửa mẫu' : 'Thêm mẫu'}</h2></div>
                <ModelForm initial={editing} onSubmit={handleSubmit} onCancel={()=>setEditing(null)} />
              </div>
            )}

          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
