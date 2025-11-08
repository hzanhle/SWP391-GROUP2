import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import DocumentUploader from '../components/DocumentUploader'
import * as clientApi from '../api/client'

export default function Profile() {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  // CCCD document states
  const [citizenFormData, setCitizenFormData] = useState({
    citizenId: '',
    fullName: '',
    sex: 'Nam',
    dayOfBirth: '',
    address: '',
    citiRegisDate: '',
    citiRegisOffice: '',
  })
  const [citizenLicenseFront, setCitizenLicenseFront] = useState(null)
  const [citizenLicenseBack, setCitizenLicenseBack] = useState(null)
  const [submittingCitizen, setSubmittingCitizen] = useState(false)
  const [citizenSuccess, setCitizenSuccess] = useState(false)
  const [citizenError, setCitizenError] = useState('')

  // GPLX document states
  const [licenseFormData, setLicenseFormData] = useState({
    licenseId: '',
    licenseType: 'B1',
    registerDate: '',
    registerOffice: '',
    sex: 'Nam',
    dayOfBirth: '',
    fullName: '',
    address: '',
  })
  const [idFront, setIdFront] = useState(null)
  const [idBack, setIdBack] = useState(null)
  const [submittingLicense, setSubmittingLicense] = useState(false)
  const [licenseSuccess, setLicenseSuccess] = useState(false)
  const [licenseError, setLicenseError] = useState('')

  // Loaded document data
  const [citizenInfo, setCitizenInfo] = useState(null)
  const [driverLicense, setDriverLicense] = useState(null)
  const [docsLoading, setDocsLoading] = useState(false)

  const authToken = typeof localStorage !== 'undefined' ? localStorage.getItem('auth.token') : null
  const authUser = typeof localStorage !== 'undefined' ? localStorage.getItem('auth.user') : null

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        setLoading(true)
        if (!authToken || !authUser) {
          setError('Please log in')
          window.location.hash = 'login'
          return
        }

        const userData = JSON.parse(authUser)
        const userId = Number(userData?.userId || userData?.UserId || userData?.id || userData?.Id)

        if (!userId || isNaN(userId)) {
          setError('Unable to determine user ID')
          return
        }

        const { data } = await clientApi.getUserDetail(userId, authToken)
        setUser(data)
        setError(null)
        await loadDocuments()
      } catch (err) {
        console.error('Error fetching profile:', err)
        setError(err.message || 'Failed to load profile information')
      } finally {
        setLoading(false)
      }
    }

    fetchUserProfile()
  }, [])

  const handleCitizenInputChange = (e) => {
    const { name, value } = e.target
    setCitizenFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleLicenseInputChange = (e) => {
    const { name, value } = e.target
    setLicenseFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const formatDate = (val) => {
    if (!val) return ''
    try { return String(val).slice(0, 10) } catch { return String(val) }
  }

  const getStatusBadgeClass = (status) => {
    const s = String(status ?? '').toLowerCase()
    if (s.includes('approve')) return 'badge green'
    if (s.includes('reject')) return 'badge red'
    if (s.includes('pend')) return 'badge gray'
    return 'badge gray'
  }

  const loadDocuments = async () => {
    try {
      setDocsLoading(true)
      const [ciRes, dlRes] = await Promise.all([
        clientApi.getCitizenInfo(authToken),
        clientApi.getDriverLicense(authToken)
      ])
      setCitizenInfo(ciRes?.data ?? ciRes)
      setDriverLicense(dlRes?.data ?? dlRes)
    } catch (e) {
      // ignore if not found or unauthorized
    } finally {
      setDocsLoading(false)
    }
  }


  const handleCitizenInfoSubmit = async (e) => {
    e.preventDefault()
    
    if (!citizenLicenseFront || !citizenLicenseBack) {
      setCitizenError('Please upload both sides of the ID')
      return
    }

    if (!citizenFormData.citizenId || !citizenFormData.dayOfBirth || !citizenFormData.citiRegisDate || !citizenFormData.citiRegisOffice) {
      setCitizenError('Please fill in all ID information')
      return
    }

    try {
  setSubmittingCitizen(true)
  setCitizenError('')

  const userId = Number(user?.userId || user?.UserId)
  if (!userId) throw new Error('Unable to determine user ID')

  // === LOG BEFORE SUBMIT ===
  console.group('%c[CitizenInfo Submit]', 'color: #00bfff; font-weight: bold;')
  console.log('🧾 UserId:', userId)
  console.log('🔐 Auth Token:', authToken ? '(token available)' : '❌ no token')
  console.log('📦 Payload to submit:', {
    CitizenId: citizenFormData.citizenId,
    Sex: citizenFormData.sex,
    DayOfBirth: citizenFormData.dayOfBirth,
    CitiRegisDate: citizenFormData.citiRegisDate,
    CitiRegisOffice: citizenFormData.citiRegisOffice,
    FullName: citizenFormData.fullName,
    Address: citizenFormData.address,
    Files: [citizenLicenseFront, citizenLicenseBack]
  })
  console.groupEnd()

  const res = await clientApi.createCitizenInfo({
    CitizenId: citizenFormData.citizenId,
    Sex: citizenFormData.sex,
    DayOfBirth: citizenFormData.dayOfBirth,
    CitiRegisDate: citizenFormData.citiRegisDate,
    CitiRegisOffice: citizenFormData.citiRegisOffice,
    FullName: citizenFormData.fullName,
    Address: citizenFormData.address,
    Files: [citizenLicenseFront, citizenLicenseBack]
  }, authToken)

  // === LOG RESPONSE ===
  console.group('%c[CitizenInfo Response]', 'color: #4caf50; font-weight: bold;')
  console.log('✅ Response from BE:', res)
  console.groupEnd()

  setCitizenSuccess(true)
  await loadDocuments()
  setCitizenFormData({
    citizenId: '',
    fullName: '',
    sex: 'Nam',
    dayOfBirth: '',
    address: '',
    citiRegisDate: '',
    citiRegisOffice: '',
  })
  setCitizenLicenseFront(null)
  setCitizenLicenseBack(null)

  setTimeout(() => setCitizenSuccess(false), 3000)

} catch (err) {
  console.group('%c[CitizenInfo Error]', 'color: #f44336; font-weight: bold;')
  console.error('❌ Error submitting CitizenInfo:', err)
  if (err.response) {
    console.error('📥 Error response from BE:', err.response)
  }
  console.groupEnd()

  setCitizenError(err.message || 'Failed to submit ID')

} finally {
  setSubmittingCitizen(false)
}}


  const handleDriverLicenseSubmit = async (e) => {
    e.preventDefault()
    
    if (!idFront || !idBack) {
      setLicenseError('Please upload both sides of the license')
      return
    }

    if (!licenseFormData.licenseId || !licenseFormData.registerDate || !licenseFormData.registerOffice) {
      setLicenseError('Please fill in all license information')
      return
    }

    try {
  setSubmittingLicense(true);
  setLicenseError('');

  const userId = Number(user?.userId || user?.UserId);
  if (!userId) throw new Error('Unable to determine user ID');

  // Create payload before logging
  const payload = {
    LicenseId: licenseFormData.licenseId,
    LicenseType: licenseFormData.licenseType,
    RegisterDate: licenseFormData.registerDate,
    RegisterOffice: licenseFormData.registerOffice,
    DayOfBirth: licenseFormData.dayOfBirth,
    FullName: licenseFormData.fullName,
    Sex: licenseFormData.sex,
    Address: licenseFormData.address,
    Files: [idFront, idBack],
  };

  // Log overall object
  console.log('🧾 DriverLicenseRequest payload:', payload);
  console.table(payload);

  // Log data type of each field
  for (const [key, value] of Object.entries(payload)) {
    console.log(`${key}:`, value, `→ type: ${typeof value}`);
  }

  const res = await clientApi.createDriverLicense(payload, authToken);
  console.log('✅ API Response:', res);

  setLicenseSuccess(true);
  await loadDocuments();
  setLicenseFormData({
  licenseId: '',
  licenseType: 'B1',
  registerDate: '',
  registerOffice: '',
  sex: 'Nam',           // ✅
  dayOfBirth: '',       // ✅
  fullName: '',         // ✅
  address: '',          // ✅
});
  setIdFront(null);
  setIdBack(null);

  setTimeout(() => {
    setLicenseSuccess(false);
  }, 3000);
} catch (err) {
  console.error('❌ Error submitting driver license:', err);
  setLicenseError(err.message || 'Failed to submit license');
} finally {
  setSubmittingLicense(false);
}}

  if (loading) {
    return (
      <div data-figma-layer="Profile Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="text-center" style={{ padding: '4rem 0' }}>
                <p>Loading...</p>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  return (
    <div data-figma-layer="Profile Page">
      <Navbar />
      <main>
        <section className="section page-offset">
          <div className="container">
            <div className="section-header">
              <h1 className="section-title">Personal Information</h1>
              <p className="section-subtitle">Manage your account information and verification documents.</p>
            </div>

            {error && (
              <div className="error-message error-visible" style={{ marginBottom: '1.5rem' }}>
                <span>{error}</span>
              </div>
            )}

            <div className="card">
              <div className="card-body">
                <h3 className="card-title">Personal Information</h3>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '1.5rem' }}>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Username</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.userName || user?.UserName || 'Not updated'}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Email</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.email || user?.Email || 'Not updated'}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Phone Number</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.phoneNumber || user?.PhoneNumber || 'Not updated'}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="section-header" style={{ marginTop: '3rem' }}>
              <h2 className="section-title">Verification Documents</h2>
              <p className="section-subtitle">Upload and manage documents needed to verify your identity.</p>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">National ID Card</h3>
                  <p className="card-subtext" style={{ marginTop: '1rem', marginBottom: '1.5rem' }}>
                    Fill in information and upload front and back photos
                  </p>

                  {citizenError && (
                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#f8d7da',
                      color: '#721c24',
                      borderRadius: '0.5rem',
                      marginBottom: '1.5rem',
                    }}>
                      {citizenError}
                    </div>
                  )}

                  {citizenSuccess && (
                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#d4edda',
                      color: '#155724',
                      borderRadius: '0.5rem',
                      marginBottom: '1.5rem',
                      textAlign: 'center',
                    }}>
                      ✅ ID card submitted successfully!
                    </div>
                  )}

                  {citizenInfo && (
                    <div className="doc-summary">
                      <div style={{ marginBottom: '1rem' }}>
                        <span className={getStatusBadgeClass(citizenInfo.Status || citizenInfo.status)}>
                          Status: {String(citizenInfo.Status || citizenInfo.status || 'Unknown')}
                        </span>
                      </div>
                      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                        <div>
                          <p className="label" style={{ margin: 0 }}>ID Number</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{citizenInfo.CitizenId || citizenInfo.citizenId}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Full Name</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{citizenInfo.FullName || citizenInfo.fullName}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Gender</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{citizenInfo.Sex || citizenInfo.sex}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Date of Birth</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{formatDate(citizenInfo.DayOfBirth || citizenInfo.dayOfBirth)}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Address</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{citizenInfo.Address || citizenInfo.address}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>ID Registration Date</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{formatDate(citizenInfo.CitiRegisDate || citizenInfo.citiRegisDate)}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>ID Registration Office</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{citizenInfo.CitiRegisOffice || citizenInfo.citiRegisOffice}</p>
                        </div>
                      </div>
                    </div>
                  )}

                  {!citizenInfo && (
                  <form onSubmit={handleCitizenInfoSubmit}>
                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="citizenId" className="label">ID Number</label>
                      <input
                        id="citizenId"
                        type="text"
                        name="citizenId"
                        value={citizenFormData.citizenId}
                        onChange={handleCitizenInputChange}
                        className="input"
                        placeholder="E.g: 123456789"
                        pattern="^[0-9]{9,12}$"
                        title="ID number must be 9-12 characters"
                      />
                    </div>
                     <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="fullName" className="label">Full Name</label>
                      <input
                        id="fullName"
                        type="text"
                        name="fullName"
                        value={citizenFormData.fullName}
                        onChange={handleCitizenInputChange}
                        className="input"
                        placeholder="E.g: John Smith"
                      />
                    </div> 


                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="sex" className="label">Gender</label>
                      <select
                        id="sex"
                        name="sex"
                        value={citizenFormData.sex}
                        onChange={handleCitizenInputChange}
                        className="input"
                      >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="dayOfBirth" className="label">Date of Birth</label>
                      <input
                        id="dayOfBirth"
                        type="date"
                        name="dayOfBirth"
                        value={citizenFormData.dayOfBirth}
                        onChange={handleCitizenInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="address" className="label">Address</label>
                      <input
                        id="address"
                        type="text"
                        name="address"
                        value={citizenFormData.address}
                        onChange={handleCitizenInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="citiRegisDate" className="label">ID Registration Date</label>
                      <input
                        id="citiRegisDate"
                        type="date"
                        name="citiRegisDate"
                        value={citizenFormData.citiRegisDate}
                        onChange={handleCitizenInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="citiRegisOffice" className="label">ID Registration Office</label>
                      <input
                        id="citiRegisOffice"
                        type="text"
                        name="citiRegisOffice"
                        value={citizenFormData.citiRegisOffice}
                        onChange={handleCitizenInputChange}
                        className="input"
                        placeholder="E.g: District 1 Police, HCM City"
                      />
                    </div>

                    <div className="doc-uploaders" style={{ marginBottom: '1.5rem' }}>
                      <DocumentUploader
                        label="ID - Front"
                        hint="JPG, PNG or PDF"
                        value={citizenLicenseFront}
                        onChange={setCitizenLicenseFront}
                      />
                      <DocumentUploader
                        label="ID - Back"
                        hint="JPG, PNG or PDF"
                        value={citizenLicenseBack}
                        onChange={setCitizenLicenseBack}
                      />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                      <span className={citizenLicenseFront && citizenLicenseBack ? 'badge green' : 'badge gray'}>
                        {citizenLicenseFront && citizenLicenseBack ? 'Fully uploaded' : 'Incomplete documents'}
                      </span>
                    </div>

                    <CTA
                      as="button"
                      type="submit"
                      disabled={submittingCitizen || !citizenLicenseFront || !citizenLicenseBack}
                    >
                      {submittingCitizen ? 'Submitting...' : 'Submit ID Card'}
                    </CTA>
                  </form>
                  )}
                </div>
              </div>

              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Driver License</h3>
                  <p className="card-subtext" style={{ marginTop: '1rem', marginBottom: '1.5rem' }}>
                    Fill in information and upload front and back photos
                  </p>

                  {licenseError && (
                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#f8d7da',
                      color: '#721c24',
                      borderRadius: '0.5rem',
                      marginBottom: '1.5rem',
                    }}>
                      {licenseError}
                    </div>
                  )}

                  {licenseSuccess && (
                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#d4edda',
                      color: '#155724',
                      borderRadius: '0.5rem',
                      marginBottom: '1.5rem',
                      textAlign: 'center',
                    }}>
                      ✅ Driver license submitted successfully!
                    </div>
                  )}

                  

                  {driverLicense && (
                    <div className="doc-summary">
                      <div style={{ marginBottom: '1rem' }}>
                        <span className={getStatusBadgeClass(driverLicense.Status || driverLicense.status)}>
                          Status: {String(driverLicense.Status || driverLicense.status || 'Unknown')}
                        </span>
                      </div>
                      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                        <div>
                          <p className="label" style={{ margin: 0 }}>License Number</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.LicenseId || driverLicense.licenseId}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>License Class</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.LicenseType || driverLicense.licenseType}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Full Name</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.FullName || driverLicense.fullName}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Gender</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.Sex || driverLicense.sex}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Date of Birth</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{formatDate(driverLicense.DayOfBirth || driverLicense.dayOfBirth)}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>Address</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.Address || driverLicense.address}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>License Issue Date</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{formatDate(driverLicense.RegisterDate || driverLicense.registerDate)}</p>
                        </div>
                        <div>
                          <p className="label" style={{ margin: 0 }}>License Issuing Office</p>
                          <p style={{ margin: '0.25rem 0 0 0' }}>{driverLicense.RegisterOffice || driverLicense.registerOffice}</p>
                        </div>
                      </div>
                    </div>
                  )}

                  {!driverLicense && (
                  <form onSubmit={handleDriverLicenseSubmit}>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="fullName" className="label">Full Name</label>
                      <input
                        id="fullName"
                        type="text"
                        name="fullName"
                        value={licenseFormData.fullName}
                        onChange={handleLicenseInputChange}
                        className="input"
                        placeholder="E.g: John Smith"
                      />
                    </div> 

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="sex" className="label">Gender</label>
                      <select
                        id="sex"
                        name="sex"
                        value={licenseFormData.sex}
                        onChange={handleLicenseInputChange}
                        className="input"
                      >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>

                  <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="dayOfBirth" className="label">Date of Birth</label>
                      <input
                        id="dayOfBirth"
                        type="date"
                        name="dayOfBirth"
                        value={licenseFormData.dayOfBirth}
                        onChange={handleLicenseInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="address" className="label">Address</label>
                      <input
                        id="address"
                        type="text"
                        name="address"
                        value={licenseFormData.address}
                        onChange={handleLicenseInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="licenseId" className="label">License Number</label>
                      <input
                        id="licenseId"
                        type="text"
                        name="licenseId"
                        value={licenseFormData.licenseId}
                        onChange={handleLicenseInputChange}
                        className="input"
                        placeholder="E.g: 1234567890"
                        pattern="^[0-9]{10,12}$"
                        title="License number must be 10-12 characters"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="licenseType" className="label">License Class</label>
                      <select
                        id="licenseType"
                        name="licenseType"
                        value={licenseFormData.licenseType}
                        onChange={handleLicenseInputChange}
                        className="input"
                      >
                        <option value="A1">A1</option>
                        <option value="A2">A2</option>
                        <option value="A3">A3</option>
                        <option value="A4">A4</option>
                        <option value="B1">B1</option>
                        <option value="B2">B2</option>
                        <option value="C">C</option>
                        <option value="D">D</option>
                        <option value="E">E</option>
                        <option value="F">F</option>
                        <option value="FB2">FB2</option>
                        <option value="FC">FC</option>
                      </select>
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="registerDate" className="label">License Issue Date</label>
                      <input
                        id="registerDate"
                        type="date"
                        name="registerDate"
                        value={licenseFormData.registerDate}
                        onChange={handleLicenseInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="registerOffice" className="label">License Issuing Office</label>
                      <input
                        id="registerOffice"
                        type="text"
                        name="registerOffice"
                        value={licenseFormData.registerOffice}
                        onChange={handleLicenseInputChange}
                        className="input"
                        placeholder="E.g: HCM City Police"
                      />
                    </div>

                    <div className="doc-uploaders" style={{ marginBottom: '1.5rem' }}>
                      <DocumentUploader
                        label="License - Front"
                        hint="JPG, PNG or PDF"
                        value={idFront}
                        onChange={setIdFront}
                      />
                      <DocumentUploader
                        label="License - Back"
                        hint="JPG, PNG or PDF"
                        value={idBack}
                        onChange={setIdBack}
                      />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                      <span className={idFront && idBack ? 'badge green' : 'badge gray'}>
                        {idFront && idBack ? 'Fully uploaded' : 'Incomplete documents'}
                      </span>
                    </div>

                    <CTA
                      as="button"
                      type="submit"
                      disabled={submittingLicense || !idFront || !idBack}
                    >
                      {submittingLicense ? 'Submitting...' : 'Submit License'}
                    </CTA>
                  </form>
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
