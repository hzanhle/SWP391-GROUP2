import React, { useState } from 'react'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'

export default function ProfileDocs() {
  const [licenseFront, setLicenseFront] = useState(null)
  const [licenseBack, setLicenseBack] = useState(null)
  const [idFront, setIdFront] = useState(null)
  const [idBack, setIdBack] = useState(null)
  const [submitted, setSubmitted] = useState(false)

  function handleSubmit(e) {
    e.preventDefault()
    setSubmitted(true)
    alert('Đã gửi hồ sơ để xác minh')
  }

  return (
    <div data-figma-layer="Renter Documents Page">
      <main>
        <section className="profile-section">
          <div className="profile-hero">
            <div className="profile-hero__overlay"></div>
            <div className="container">
              <div className="profile-hero__content">
                <h1 className="profile-hero__title">Hồ sơ giấy tờ</h1>
                <p className="profile-hero__subtitle">Tải lên và quản lý giấy phép lái xe, CCCD/CMND để xác minh danh tính.</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="card">
              <form className="card-body" onSubmit={handleSubmit}>
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Giấy phép lái xe</h3>
                    <p className="card-subtext">Mặt trước và mặt sau GPLX còn hiệu lực</p>
                    <div className="doc-uploaders">
                      <DocumentUploader label="GPLX - Mặt trước" hint="JPG, PNG hoặc PDF" value={licenseFront} onChange={setLicenseFront} />
                      <DocumentUploader label="GPLX - Mặt sau" hint="JPG, PNG hoặc PDF" value={licenseBack} onChange={setLicenseBack} />
                    </div>
                    <div className="row">
                      <span className={submitted ? 'badge gray' : (licenseFront && licenseBack ? 'badge green' : 'badge gray')}>
                        {submitted ? 'Đang chờ xác minh' : (licenseFront && licenseBack ? 'Đã tải đủ' : 'Chưa đủ tài liệu')}
                      </span>
                    </div>
                  </div>

                  <div className="doc-card">
                    <h3 className="card-title">CCCD / CMND</h3>
                    <p className="card-subtext">Ảnh rõ nét, không che mờ thông tin</p>
                    <div className="doc-uploaders">
                      <DocumentUploader label="CCCD - Mặt trước" hint="JPG, PNG hoặc PDF" value={idFront} onChange={setIdFront} />
                      <DocumentUploader label="CCCD - Mặt sau" hint="JPG, PNG hoặc PDF" value={idBack} onChange={setIdBack} />
                    </div>
                    <div className="row">
                      <span className={submitted ? 'badge gray' : (idFront && idBack ? 'badge green' : 'badge gray')}>
                        {submitted ? 'Đang chờ xác minh' : (idFront && idBack ? 'Đã tải đủ' : 'Chưa đủ tài liệu')}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#profile">Quay lại hồ sơ</a>
                  <CTA as="button" type="submit">Gửi xác minh</CTA>
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
