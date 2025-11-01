import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as clientApi from '../api/client'

export default function Profile() {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [success, setSuccess] = useState(false)
  const [editing, setEditing] = useState(false)
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
  })
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        setLoading(true)
        const authToken = localStorage.getItem('auth.token')
        const authUser = localStorage.getItem('auth.user')

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

        const { data } = await clientApi.getUserById(userId, authToken)
        setUser(data)
        setFormData({
          fullName: data?.fullName || data?.FullName || '',
          email: data?.email || data?.Email || '',
          phoneNumber: data?.phoneNumber || data?.PhoneNumber || '',
          address: data?.address || data?.Address || '',
        })
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

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    
    try {
      setSubmitting(true)
      setError(null)
      
      const authToken = localStorage.getItem('auth.token')
      
      await clientApi.updateCitizenInfo({
        UserId: user?.userId || user?.UserId,
        FullName: formData.fullName,
        Address: formData.address,
      }, authToken)
      
      setSuccess(true)
      setEditing(false)
      
      setTimeout(() => {
        setSuccess(false)
      }, 3000)
    } catch (err) {
      console.error('Error updating profile:', err)
      setError(err.message || 'Không cập nhật được hồ sơ')
    } finally {
      setSubmitting(false)
    }
  }

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
              <h1 className="section-title">Personal Information</h1>
              <p className="section-subtitle">Manage your account information and verification documents.</p>
            </div>

            {error && (
              <div className="error-message error-visible" style={{ marginBottom: '1.5rem' }}>
                <span>{error}</span>
              </div>
            )}

            {success && (
              <div style={{
                padding: '1rem',
                backgroundColor: '#d4edda',
                color: '#155724',
                borderRadius: '0.5rem',
                marginBottom: '1.5rem',
                textAlign: 'center',
              }}>
                ✅ Updated successfully!
              </div>
            )}

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
              <div className="card">
                <div className="card-body">
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                    <h3 className="card-title">Thông tin cá nhân</h3>
                    {!editing && (
                      <button
                        onClick={() => setEditing(true)}
                        style={{
                          padding: '0.5rem 1rem',
                          backgroundColor: '#ff4d30',
                          color: 'white',
                          border: 'none',
                          borderRadius: '0.4rem',
                          cursor: 'pointer',
                          fontSize: '1.2rem',
                        }}
                      >
                        ✏️ Edit
                      </button>
                    )}
                  </div>

                  {editing ? (
                    <form onSubmit={handleSubmit}>
                      <div style={{ marginBottom: '1.5rem' }}>
                        <label htmlFor="fullName" className="label">Full Name</label>
                        <input
                          id="fullName"
                          type="text"
                          name="fullName"
                          value={formData.fullName}
                          onChange={handleInputChange}
                          className="input"
                          placeholder="Enter full name"
                        />
                      </div>

                      <div style={{ marginBottom: '1.5rem' }}>
                        <label htmlFor="email" className="label">Email</label>
                        <input
                          id="email"
                          type="email"
                          value={formData.email}
                          className="input"
                          disabled
                          style={{ backgroundColor: '#f9f9f9', cursor: 'not-allowed' }}
                        />
                        <p className="card-subtext" style={{ marginTop: '0.5rem' }}>Email cannot be changed</p>
                      </div>

                      <div style={{ marginBottom: '1.5rem' }}>
                        <label htmlFor="phoneNumber" className="label">Phone Number</label>
                        <input
                          id="phoneNumber"
                          type="tel"
                          value={formData.phoneNumber}
                          className="input"
                          disabled
                          style={{ backgroundColor: '#f9f9f9', cursor: 'not-allowed' }}
                        />
                        <p className="card-subtext" style={{ marginTop: '0.5rem' }}>Contact support to change</p>
                      </div>

                      <div style={{ marginBottom: '1.5rem' }}>
                        <label htmlFor="address" className="label">Address</label>
                        <textarea
                          id="address"
                          name="address"
                          value={formData.address}
                          onChange={handleInputChange}
                          className="input"
                          placeholder="Enter address"
                          style={{ minHeight: '80px' }}
                        />
                      </div>

                      <div style={{ display: 'flex', gap: '1rem' }}>
                        <CTA
                          as="button"
                          type="submit"
                          disabled={submitting}
                        >
                          {submitting ? 'Saving...' : 'Save Changes'}
                        </CTA>
                        <button
                          type="button"
                          onClick={() => setEditing(false)}
                          style={{
                            padding: '0.75rem 1.5rem',
                            backgroundColor: '#f0f0f0',
                            color: '#333',
                            border: '1px solid #ddd',
                            borderRadius: '0.4rem',
                            cursor: 'pointer',
                            fontSize: '1.4rem',
                          }}
                        >
                          Cancel
                        </button>
                      </div>
                    </form>
                  ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                      <div>
                        <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Full Name</p>
                        <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                          {user?.fullName || user?.FullName || 'Not updated'}
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
                      <div>
                        <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Address</p>
                        <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                          {user?.address || user?.Address || 'Not updated'}
                        </p>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              <div className="card">
                <div className="card-body">
                  <h3 className="card-title">Document Management</h3>
                  <p className="card-subtext" style={{ marginTop: '1rem', marginBottom: '1.5rem' }}>
                    Upload and manage your verification documents.
                  </p>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#f5f5f5',
                      borderRadius: '0.5rem',
                      border: '1px solid #ddd',
                    }}>
                      <p style={{ margin: 0, fontWeight: '500', marginBottom: '0.5rem' }}>Driver's License</p>
                      <p style={{ margin: '0 0 0.75rem 0', fontSize: '1.2rem', color: '#666' }}>
                        Upload front and back photos
                      </p>
                      <CTA as="a" href="#profile-docs" variant="secondary" style={{ display: 'inline-block' }}>
                        Manage License
                      </CTA>
                    </div>

                    <div style={{
                      padding: '1rem',
                      backgroundColor: '#f5f5f5',
                      borderRadius: '0.5rem',
                      border: '1px solid #ddd',
                    }}>
                      <p style={{ margin: 0, fontWeight: '500', marginBottom: '0.5rem' }}>ID Card</p>
                      <p style={{ margin: '0 0 0.75rem 0', fontSize: '1.2rem', color: '#666' }}>
                        Upload front and back photos
                      </p>
                      <CTA as="a" href="#profile-docs" variant="secondary" style={{ display: 'inline-block' }}>
                        Manage ID Card
                      </CTA>
                    </div>
                  </div>
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
