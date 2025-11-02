import React from 'react'
import '../styles/TermsAndServices.css'

export default function TermsAndServices({ isOpen, onClose }) {
  if (!isOpen) return null

  return (
    <div className="terms-overlay">
      <div className="terms-modal">
        <div className="terms-header">
          <h2 className="terms-title">Điều khoản &amp; Dịch vụ</h2>
          <button 
            className="terms-close-btn" 
            onClick={onClose}
            aria-label="Close terms and services"
          >
            ✕
          </button>
        </div>
        
        <div className="terms-content">
          <section className="terms-section">
            <h3>1. Thời gian chuẩn bị</h3>
            <p>
              Khách hàng có tối đa 15 phút để đến điểm nhận xe sau thời gian được chỉ định. 
              Nếu trễ hơn 15 phút, đơn hàng sẽ bị hủy và có thể mất phí.
            </p>
          </section>

          <section className="terms-section">
            <h3>2. Thời gian thuê xe</h3>
            <p>
              Thời gian đặt xe và trả xe phải cách nhau ít nhất 3 tiếng. 
              Thời gian thuê xe bắt đầu từ lúc nhận xe tại điểm nhận và kết thúc khi trả xe tại điểm trả được chỉ định.
            </p>
          </section>

          <section className="terms-section">
            <h3>3. Tiền cọc và thanh toán</h3>
            <p>
              Tiền cọc sẽ được giữ lại trong suốt quá trình thuê. 
              Nếu xe bị hư hỏng hoặc mất mát, tiền cọc sẽ được dùng để bù đắp thiệt hại.
            </p>
          </section>

          <section className="terms-section">
            <h3>4. Trách nhiệm và bảo hiểm</h3>
            <p>
              Người thuê xe chịu trách nhiệm về tất cả các hư hỏng, mất mát hoặc vi phạm luật lệ giao thông. 
              Chiếc xe được bảo hiểm cơ bản. Chi tiết bảo hiểm sẽ được cung cấp tại thời điểm nhận xe.
            </p>
          </section>

          <section className="terms-section">
            <h3>5. Hủy đơn hàng</h3>
            <p>
              Khách hàng có thể hủy đơn hàng miễn phí nếu hủy trước 24 giờ. 
              Hủy sau 24 giờ sẽ mất 50% chi phí thuê. Hủy sau khi nhận xe sẽ mất 100% chi phí.
            </p>
          </section>

          <section className="terms-section">
            <h3>6. Quy tắc giao thông</h3>
            <p>
              Người thuê xe phải tuân thủ tất cả luật lệ giao thông và an toàn đường bộ. 
              Bất kỳ vi phạm nào sẽ được thanh toán bởi người thuê.
            </p>
          </section>

          <section className="terms-section">
            <h3>7. Bảo trì và vệ sinh</h3>
            <p>
              Xe phải được trả lại trong tình trạng sạch sẽ và đầy nhiên liệu. 
              Nếu không, sẽ có phí bảo trì hoặc vệ sinh bổ sung.
            </p>
          </section>

          <section className="terms-section">
            <h3>8. Liên hệ hỗ trợ</h3>
            <p>
              Nếu có bất kỳ câu hỏi hoặc vấn đề nào trong quá trình thuê, vui lòng liên hệ ngay với đội hỗ trợ khách hàng của chúng tôi.
            </p>
          </section>
        </div>

        <div className="terms-footer">
          <button 
            className="btn btn-secondary" 
            onClick={onClose}
          >
            Đóng
          </button>
        </div>
      </div>
    </div>
  )
}
