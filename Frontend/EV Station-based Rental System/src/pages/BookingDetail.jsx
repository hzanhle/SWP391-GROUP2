import React, { useState, useEffect } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import * as bookingApi from '../api/booking'

export default function BookingDetail() {
  const [order, setOrder] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchOrderDetails = async () => {
      try {
        setLoading(true)
        // Extract query params from hash (e.g., #booking?orderId=123)
        const hash = window.location.hash.substring(1)
        const hashParams = new URLSearchParams(hash.split('?')[1] || '')
        const orderIdParam = hashParams.get('orderId')
        const pendingBooking = JSON.parse(localStorage.getItem('pending_booking') || '{}')
        const activeOrder = localStorage.getItem('active_order')
        const orderId = Number(orderIdParam || pendingBooking.orderId || activeOrder)

        console.log('[BookingDetail] Hash:', hash, 'OrderIdParam:', orderIdParam, 'PendingOrderId:', pendingBooking.orderId, 'ActiveOrder:', activeOrder, 'FinalOrderId:', orderId)

        if (!orderId || isNaN(orderId)) {
          setError('Không tìm thấy mã đơn hàng')
          return
        }

        const authToken = localStorage.getItem('auth.token')
        if (!authToken) {
          setError('Vui lòng đăng nhập')
          window.location.hash = 'login'
          return
        }

        const { data } = await bookingApi.getOrderById(orderId, authToken)
        setOrder(data)
        setError(null)
      } catch (err) {
        console.error('Error fetching order details:', err)
        setError(err.message || 'Không tải được chi tiết đơn hàng')
      } finally {
        setLoading(false)
      }
    }

    fetchOrderDetails()

    // Listen for hash changes to refetch when user navigates to different order
    const handleHashChange = () => {
      fetchOrderDetails()
    }
    window.addEventListener('hashchange', handleHashChange)
    return () => window.removeEventListener('hashchange', handleHashChange)
  }, [])

  const getStatusBadgeClass = (status) => {
    const statusMap = {
      'Pending': 'yellow',
      'Confirmed': 'blue',
      'Active': 'green',
      'Completed': 'green',
      'Cancelled': 'red',
    }
    return statusMap[status] || 'gray'
  }

  const getNextActions = (status) => {
    const currentStatus = status || order?.status || order?.Status
    if (currentStatus === 'Confirmed' || currentStatus === 'Pending') {
      return (
        <>
          <CTA as="a" href={`#check-in?orderId=${order?.orderId || order?.OrderId}`} variant="primary">Check-in nhận xe</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Đặt xe mới</CTA>
        </>
      )
    }
    if (currentStatus === 'Active') {
      return (
        <>
          <CTA as="a" href="#return" variant="primary">Trả xe</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Đặt xe mới</CTA>
        </>
      )
    }
    if (currentStatus === 'Completed') {
      return (
        <>
          <CTA as="a" href="#feedback" variant="primary">Gửi phản hồi</CTA>
          <CTA as="a" href="#booking-new" variant="secondary">Đặt xe mới</CTA>
        </>
      )
    }
    return (
      <CTA as="a" href="#booking-new" variant="primary">Đặt xe mới</CTA>
    )
  }

  if (loading) {
    return (
      <div data-figma-layer="Booking Detail Page">
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

  if (error) {
    return (
      <div data-figma-layer="Booking Detail Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body">
                  <div className="error-message">
                    <span>{error}</span>
                  </div>
                  <CTA as="a" href="#booking" style={{ marginTop: '1rem' }}>Quay lại danh sách</CTA>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  if (!order) {
    return (
      <div data-figma-layer="Booking Detail Page">
        <Navbar />
        <main>
          <section className="section page-offset">
            <div className="container">
              <div className="card">
                <div className="card-body text-center">
                  <p>Không tìm thấy thông tin đơn hàng</p>
                  <CTA as="a" href="#booking" style={{ marginTop: '1rem' }}>Quay lại</CTA>
                </div>
              </div>
            </div>
          </section>
        </main>
        <Footer />
      </div>
    )
  }

  const status = order?.status || order?.Status || 'Unknown'
  const orderId = order?.orderId || order?.OrderId
  const fromDate = new Date(order?.fromDate || order?.FromDate)
  const toDate = new Date(order?.toDate || order?.ToDate)

  return (
    <div data-figma-layer="Booking Detail Page">
      <Navbar />
      <main>
        <section id="booking" className="section page-offset" aria-labelledby="booking-detail-title">
          <div className="container">
            <div className="section-header">
              <h1 id="booking-detail-title" className="section-title">Chi tiết đặt xe</h1>
              <p className="section-subtitle">Theo dõi trạng thái và thao tác tiếp theo.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <div className="row-between" style={{ marginBottom: '1.5rem' }}>
                  <h3 className="card-title">Mã đơn: #{orderId}</h3>
                  <span className={`badge ${getStatusBadgeClass(status)}`}>{status}</span>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '2rem' }}>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Thời gian nhận xe</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {fromDate.toLocaleString('vi-VN')}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Thời gian trả xe</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {toDate.toLocaleString('vi-VN')}
                    </p>
                  </div>
                </div>

                {order?.vehicle && (
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '2rem' }}>
                    <div>
                      <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Xe</p>
                      <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                        {order.vehicle.model || order.Vehicle?.Model || 'N/A'}
                      </p>
                    </div>
                    <div>
                      <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Màu xe</p>
                      <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                        {order.vehicle.color || order.Vehicle?.Color || 'N/A'}
                      </p>
                    </div>
                  </div>
                )}

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '2rem' }}>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Tổng tiền</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.6rem', fontWeight: '500', color: '#ff4d30' }}>
                      ${Number(order?.totalCost || order?.TotalCost || 0).toFixed(2)}
                    </p>
                  </div>
                  <div>
                    <p style={{ margin: 0, color: '#999', fontSize: '1.2rem' }}>Phương thức thanh toán</p>
                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '1.4rem', fontWeight: '500' }}>
                      {order?.paymentMethod || order?.PaymentMethod || 'VNPay'}
                    </p>
                  </div>
                </div>

                <hr style={{ margin: '2rem 0' }} />

                <div className="row" aria-label="Next actions" style={{ gap: '1rem', flexWrap: 'wrap' }}>
                  {getNextActions(status)}
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
