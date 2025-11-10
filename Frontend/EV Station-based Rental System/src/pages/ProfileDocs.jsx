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
    alert('Profile submitted for verification')
  }

  return (
    <div data-figma-layer="Renter Documents Page">
      <main>
        <section className="profile-section page-offset">
          <div className="profile-hero">
            <div className="profile-hero__overlay"></div>
            <div className="container">
              <div className="profile-hero__content">
                <h1 className="profile-hero__title">Document Profile</h1>
                <p className="profile-hero__subtitle">Upload and manage your driver's license and ID card for identity verification.</p>
              </div>
            </div>
          </div>

          <div className="container">
            <div className="card">
              <form className="card-body" onSubmit={handleSubmit}>
                <div className="docs-grid">
                  <div className="doc-card">
                    <h3 className="card-title">Driver's License</h3>
                    <p className="card-subtext">Front and back of valid driver's license</p>
                    <div className="doc-uploaders">
                      <DocumentUploader label="License - Front" hint="JPG, PNG or PDF" value={licenseFront} onChange={setLicenseFront} />
                      <DocumentUploader label="License - Back" hint="JPG, PNG or PDF" value={licenseBack} onChange={setLicenseBack} />
                    </div>
                    <div className="row">
                      <span className={submitted ? 'badge gray' : (licenseFront && licenseBack ? 'badge green' : 'badge gray')}>
                        {submitted ? 'Pending verification' : (licenseFront && licenseBack ? 'Uploaded complete' : 'Incomplete documents')}
                      </span>
                    </div>
                  </div>

                  <div className="doc-card">
                    <h3 className="card-title">ID Card</h3>
                    <p className="card-subtext">Clear photos, no information obscured</p>
                    <div className="doc-uploaders">
                      <DocumentUploader label="ID - Front" hint="JPG, PNG or PDF" value={idFront} onChange={setIdFront} />
                      <DocumentUploader label="ID - Back" hint="JPG, PNG or PDF" value={idBack} onChange={setIdBack} />
                    </div>
                    <div className="row">
                      <span className={submitted ? 'badge gray' : (idFront && idBack ? 'badge green' : 'badge gray')}>
                        {submitted ? 'Pending verification' : (idFront && idBack ? 'Uploaded complete' : 'Incomplete documents')}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#profile">Back to Profile</a>
                  <CTA as="button" type="submit">Submit Verification</CTA>
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
