import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import Stepper from '../components/Stepper'

export default function BookingNew() {
  const [step, setStep] = useState(0)
  const steps = ['Chọn điểm thuê', 'Chọn xe', 'Lịch & xác nhận']

  function next() { setStep((s) => Math.min(s + 1, steps.length - 1)) }
  function back() { setStep((s) => Math.max(s - 1, 0)) }

  function confirm() {
    alert('Đã tạo đơn đặt xe')
    window.location.hash = 'booking'
  }

  return (
    <div data-figma-layer="Booking New Page">
      <Navbar />
      <main>
        <section id="booking-new" className="section" aria-labelledby="booking-title">
          <div className="container">
            <div className="section-header">
              <h1 id="booking-title" className="section-title">Đặt xe mới</h1>
              <p className="section-subtitle">Hoàn thành 3 bước đơn giản để đặt xe.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <Stepper steps={steps} current={step} />

                {step === 0 && (
                  <div className="field">
                    <label htmlFor="station" className="label">Điểm thuê</label>
                    <select id="station" className="select" defaultValue="Central Hub">
                      <option>Central Hub</option>
                      <option>Riverside</option>
                      <option>Airport West</option>
                    </select>
                  </div>
                )}

                {step === 1 && (
                  <div className="field">
                    <label htmlFor="vehicle" className="label">Chọn xe</label>
                    <select id="vehicle" className="select" defaultValue="Tesla Model 3">
                      <option>Tesla Model 3</option>
                      <option>Nissan Leaf</option>
                      <option>Hyundai Ioniq 5</option>
                    </select>
                  </div>
                )}

                {step === 2 && (
                  <div className="booking-grid">
                    <div className="field">
                      <label htmlFor="start" className="label">Thời gian bắt đầu</label>
                      <input id="start" type="datetime-local" className="input" />
                    </div>
                    <div className="field">
                      <label htmlFor="end" className="label">Thời gian kết thúc</label>
                      <input id="end" type="datetime-local" className="input" />
                    </div>
                    <div className="summary">
                      <h3 className="card-title">Tóm tắt</h3>
                      <p className="card-subtext">Địa điểm: Central Hub</p>
                      <p className="card-subtext">Xe: Tesla Model 3</p>
                      <p className="card-subtext">Giá dự kiến: $48</p>
                    </div>
                  </div>
                )}

                <div className="row-between">
                  <CTA as="button" variant="ghost" onClick={back} aria-disabled={step===0}>Quay lại</CTA>
                  {step < steps.length - 1 ? (
                    <CTA as="button" onClick={next}>Tiếp tục</CTA>
                  ) : (
                    <CTA as="button" onClick={confirm}>Xác nhận đặt</CTA>
                  )}
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
