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
    sex: 'Nam',
    dayOfBirth: '',
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
  })
  const [idFront, setIdFront] = useState(null)
  const [idBack, setIdBack] = useState(null)
  const [submittingLicense, setSubmittingLicense] = useState(false)
  const [licenseSuccess, setLicenseSuccess] = useState(false)
  const [licenseError, setLicenseError] = useState('')

  const authToken = typeof localStorage !== 'undefined' ? localStorage.getItem('auth.token') : null
  const authUser = typeof localStorage !== 'undefined' ? localStorage.getItem('auth.user') : null

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        setLoading(true)
        if (!authToken || !authUser) {
          setError('Vui lòng đăng nhập')
          window.location.hash = 'login'
          return
        }

        const userData = JSON.parse(authUser)
        const userId = Number(userData?.userId || userData?.UserId || userData?.id || userData?.Id)

        if (!userId || isNaN(userId)) {
          setError('Không thể xác định ID người dùng')
          return
        }

        const { data } = await clientApi.getUserDetail(userId, authToken)
        setUser(data)
        setError(null)
      } catch (err) {
        console.error('Error fetching profile:', err)
        setError(err.message || 'Không tải được thông tin hồ sơ')
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


  const handleCitizenInfoSubmit = async (e) => {
    e.preventDefault()
    
    if (!citizenLicenseFront || !citizenLicenseBack) {
      setCitizenError('Vui lòng tải lên cả hai mặt CCCD')
      return
    }

    if (!citizenFormData.citizenId || !citizenFormData.dayOfBirth || !citizenFormData.citiRegisDate || !citizenFormData.citiRegisOffice) {
      setCitizenError('Vui lòng điền đầy đủ thông tin CCCD')
      return
    }

    try {
  setSubmittingCitizen(true)
  setCitizenError('')

  const userId = Number(user?.userId || user?.UserId)
  if (!userId) throw new Error('Không thể xác định ID người dùng')

  // === LOG TRƯỚC KHI GỬI ===
  console.group('%c[CitizenInfo Submit]', 'color: #00bfff; font-weight: bold;')
  console.log('🧾 UserId:', userId)
  console.log('🔐 Auth Token:', authToken ? '(đã có token)' : '❌ không có token')
  console.log('📦 Payload chuẩn bị gửi:', {
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

  // === LOG PHẢN HỒI ===
  console.group('%c[CitizenInfo Response]', 'color: #4caf50; font-weight: bold;')
  console.log('✅ Response từ BE:', res)
  console.groupEnd()

  setCitizenSuccess(true)
  setCitizenFormData({
    citizenId: '',
    sex: 'Nam',
    dayOfBirth: '',
    citiRegisDate: '',
    citiRegisOffice: '',
  })
  setCitizenLicenseFront(null)
  setCitizenLicenseBack(null)

  setTimeout(() => setCitizenSuccess(false), 3000)

} catch (err) {
  console.group('%c[CitizenInfo Error]', 'color: #f44336; font-weight: bold;')
  console.error('❌ Lỗi khi gửi CitizenInfo:', err)
  if (err.response) {
    console.error('📥 Response lỗi từ BE:', err.response)
  }
  console.groupEnd()

  setCitizenError(err.message || 'Không gửi được CCCD')

} finally {
  setSubmittingCitizen(false)
}}


  const handleDriverLicenseSubmit = async (e) => {
    e.preventDefault()
    
    if (!idFront || !idBack) {
      setLicenseError('Vui lòng tải lên cả hai mặt GPLX')
      return
    }

    if (!licenseFormData.licenseId || !licenseFormData.registerDate || !licenseFormData.registerOffice) {
      setLicenseError('Vui lòng điền đầy đủ thông tin GPLX')
      return
    }

    try {
  setSubmittingLicense(true);
  setLicenseError('');

  const userId = Number(user?.userId || user?.UserId);
  if (!userId) throw new Error('Không thể xác định ID người dùng');

  // Tạo payload trước để log dễ
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

  // Log object tổng thể
  console.log('🧾 DriverLicenseRequest payload:', payload);
  console.table(payload);

  // Log kiểu dữ liệu từng trường
  for (const [key, value] of Object.entries(payload)) {
    console.log(`${key}:`, value, `→ type: ${typeof value}`);
  }

  const res = await clientApi.createDriverLicense(payload, authToken);
  console.log('✅ API Response:', res);

  setLicenseSuccess(true);
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
  setLicenseError(err.message || 'Không gửi được GPLX');
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
                <p>Đang tải...</p>
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
              <h1 className="section-title">Thông tin cá nhân</h1>
              <p className="section-subtitle">Quản lý thông tin tài khoản và tài liệu xác minh của bạn.</p>
            </div>

            {error && (
              <div className="error-message error-visible" style={{ marginBottom: '1.5rem' }}>
                <span>{error}</span>
              </div>
            )}

            <div className="card">
              <div className="card-body">
                <h3 className="card-title">Thông tin cá nhân</h3>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '1.5rem' }}>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Tên đăng nhập</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.userName || user?.UserName || 'Chưa cập nhật'}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Email</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.email || user?.Email || 'Chưa cập nhật'}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Số điện thoại</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {user?.phoneNumber || user?.PhoneNumber || 'Chưa cập nhật'}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="section-header" style={{ marginTop: '3rem' }}>
              <h2 className="section-title">Tài liệu xác minh</h2>
              <p className="section-subtitle">Tải lên và quản lý các giấy tờ cần thiết để xác minh danh tính.</p>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">CCCD / CMND</h3>
                  <p className="card-subtext" style={{ marginTop: '1rem', marginBottom: '1.5rem' }}>
                    Điền thông tin và tải lên ảnh mặt trước, mặt sau
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
                      ✅ Gửi CCCD thành công!
                    </div>
                  )}

                  <form onSubmit={handleCitizenInfoSubmit}>
                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="citizenId" className="label">Số CCCD</label>
                      <input
                        id="citizenId"
                        type="text"
                        name="citizenId"
                        value={citizenFormData.citizenId}
                        onChange={handleCitizenInputChange}
                        className="input"
                        placeholder="VD: 123456789"
                        pattern="^[0-9]{9,12}$"
                        title="S��� CCCD phải từ 9-12 ký tự"
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
                        placeholder="VD: Le Nguyen Hoang Anh"
                      />
                    </div> 


                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="sex" className="label">Giới tính</label>
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
                      <label htmlFor="dayOfBirth" className="label">Ngày sinh</label>
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
                      <label htmlFor="address" className="label">Địa chỉ</label>
                      <input
                        id="address"
                        type="text"
                        name="address"
                        value={citizenFormData.Address}
                        onChange={handleCitizenInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="citiRegisDate" className="label">Ngày đăng ký CCCD</label>
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
                      <label htmlFor="citiRegisOffice" className="label">Nơi đăng ký CCCD</label>
                      <input
                        id="citiRegisOffice"
                        type="text"
                        name="citiRegisOffice"
                        value={citizenFormData.citiRegisOffice}
                        onChange={handleCitizenInputChange}
                        className="input"
                        placeholder="VD: Công an Quận 1, TP HCM"
                      />
                    </div>

                    <div className="doc-uploaders" style={{ marginBottom: '1.5rem' }}>
                      <DocumentUploader
                        label="CCCD - Mặt trước"
                        hint="JPG, PNG hoặc PDF"
                        value={citizenLicenseFront}
                        onChange={setCitizenLicenseFront}
                      />
                      <DocumentUploader
                        label="CCCD - Mặt sau"
                        hint="JPG, PNG hoặc PDF"
                        value={citizenLicenseBack}
                        onChange={setCitizenLicenseBack}
                      />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                      <span className={citizenLicenseFront && citizenLicenseBack ? 'badge green' : 'badge gray'}>
                        {citizenLicenseFront && citizenLicenseBack ? 'Đã tải đủ' : 'Chưa đủ tài liệu'}
                      </span>
                    </div>

                    <CTA
                      as="button"
                      type="submit"
                      disabled={submittingCitizen || !citizenLicenseFront || !citizenLicenseBack}
                    >
                      {submittingCitizen ? 'Đang gửi...' : 'Gửi CCCD'}
                    </CTA>
                  </form>
                </div>
              </div>

              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Giấy phép lái xe</h3>
                  <p className="card-subtext" style={{ marginTop: '1rem', marginBottom: '1.5rem' }}>
                    Điền thông tin và tải lên ảnh mặt trước, mặt sau
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
                      ✅ Gửi GPLX thành công!
                    </div>
                  )}

                  

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
                        placeholder="VD: Le Nguyen Hoang Anh"
                      />
                    </div> 

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="sex" className="label">Giới tính</label>
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
                      <label htmlFor="dayOfBirth" className="label">Ngày sinh</label>
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
                      <label htmlFor="address" className="label">Địa chỉ</label>
                      <input
                        id="address"
                        type="text"
                        name="address"
                        value={licenseFormData.Address}
                        onChange={handleLicenseInputChange}
                        className="input"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="licenseId" className="label">Số GPLX</label>
                      <input
                        id="licenseId"
                        type="text"
                        name="licenseId"
                        value={licenseFormData.licenseId}
                        onChange={handleLicenseInputChange}
                        className="input"
                        placeholder="VD: 1234567890"
                        pattern="^[0-9]{10,12}$"
                        title="Số GPLX phải từ 10-12 ký tự"
                      />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                      <label htmlFor="licenseType" className="label">Hạng bằng</label>
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
                      <label htmlFor="registerDate" className="label">Ngày cấp GPLX</label>
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
                      <label htmlFor="registerOffice" className="label">Nơi cấp GPLX</label>
                      <input
                        id="registerOffice"
                        type="text"
                        name="registerOffice"
                        value={licenseFormData.registerOffice}
                        onChange={handleLicenseInputChange}
                        className="input"
                        placeholder="VD: Công an TP HCM"
                      />
                    </div>

                    <div className="doc-uploaders" style={{ marginBottom: '1.5rem' }}>
                      <DocumentUploader
                        label="GPLX - Mặt trước"
                        hint="JPG, PNG hoặc PDF"
                        value={idFront}
                        onChange={setIdFront}
                      />
                      <DocumentUploader
                        label="GPLX - Mặt sau"
                        hint="JPG, PNG hoặc PDF"
                        value={idBack}
                        onChange={setIdBack}
                      />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                      <span className={idFront && idBack ? 'badge green' : 'badge gray'}>
                        {idFront && idBack ? 'Đã tải đủ' : 'Chưa đủ tài liệu'}
                      </span>
                    </div>

                    <CTA
                      as="button"
                      type="submit"
                      disabled={submittingLicense || !idFront || !idBack}
                    >
                      {submittingLicense ? 'Đang gửi...' : 'Gửi GPLX'}
                    </CTA>
                  </form>
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
