import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'

export default function Return() {
  return (
    <div data-figma-layer="Return Vehicle Page">
      <Navbar />
      <main>
        <section id="return" className="section" aria-labelledby="return-title">
          <div className="container">
            <div className="section-header">
              <h1 id="return-title" className="section-title">Trả xe</h1>
              <p className="section-subtitle">Kiểm tra tình trạng và hoàn tất thanh toán nếu phát sinh.</p>
            </div>

            <div className="card">
              <form className="card-body">
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Ảnh sau khi sử dụng</h3>
                    <div className="doc-uploaders">
                      <DocumentUploader label="Mặt trước" />
                      <DocumentUploader label="Mặt sau" />
                      <DocumentUploader label="Bên trái" />
                      <DocumentUploader label="Bên phải" />
                    </div>
                  </div>
                  <div className="doc-card">
                    <h3 className="card-title">Checklist</h3>
                    <label className="row"><input type="checkbox" required /> Không trầy xước mới</label>
                    <label className="row"><input type="checkbox" required /> Mức pin theo hợp đồng</label>
                    <label className="row"><input type="checkbox" required /> Phụ kiện đầy đủ</label>
                  </div>
                </div>
                <div className="row-between">
                  <a className="nav-link" href="#booking">Quay lại</a>
                  <CTA as="button" type="submit">Xác nhận trả xe</CTA>
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
