import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'

export default function CheckIn() {
  return (
    <div data-figma-layer="Check-in Page">
      <Navbar />
      <main>
        <section id="check-in" className="section" aria-labelledby="checkin-title">
          <div className="container">
            <div className="section-header">
              <h1 id="checkin-title" className="section-title">Check-in nhận xe</h1>
              <p className="section-subtitle">Chụp ảnh và xác nhận tình trạng trước khi nhận xe.</p>
            </div>

            <div className="card">
              <form className="card-body">
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Ảnh tình trạng xe</h3>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Mặt trước" />
                      <DocumentUploader label="Mặt sau" />
                      <DocumentUploader label="Bên trái" />
                      <DocumentUploader label="Bên phải" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Xác nhận</h3>
                    <p className="card-subtext">Tôi xác nhận đã kiểm tra xe và đồng ý điều khoản thuê.</p>
                    <label className="row"><input type="checkbox" required aria-label="Agree" /> Tôi đồng ý</label>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Quay lại</a>
                  <CTA as="button" type="submit">Hoàn tất check-in</CTA>
                </div>
              </form>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
