import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'

export default function BookingDetail() {
  const status = 'Chờ nhận xe'
  return (
    <div data-figma-layer="Booking Detail Page">
      <Navbar />
      <main>
        <section id="booking" className="section" aria-labelledby="booking-detail-title">
          <div className="container">
            <div className="section-header">
              <h1 id="booking-detail-title" className="section-title">Chi tiết đặt xe</h1>
              <p className="section-subtitle">Theo dõi trạng thái và thao tác tiếp theo.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <div className="row-between">
                  <h3 className="card-title">Mã đơn: BK-2024-0001</h3>
                  <span className="badge yellow">{status}</span>
                </div>
                <p className="card-subtext">Điểm: Central Hub • Xe: Tesla Model 3 • Thời gian: 10:00–14:00</p>
                <div className="row" aria-label="Next actions">
                  <CTA as="a" href="#check-in" variant="primary">Check-in</CTA>
                  <CTA as="a" href="#return" variant="secondary">Trả xe</CTA>
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
