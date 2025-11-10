import * as userApi from '../src/api/client';

/**
 * Check if user has all required documents for booking
 * Required documents:
 * - Citizen Info (CCCD/CMND with front and back)
 * - Driver License (Giấy phép lái xe with front and back)
 * @param {number} userId - User ID
 * @param {string} token - JWT token
 * @returns {Promise<{hasAllDocuments: boolean, missingDocs: string[], citizenInfo: object|null, driverLicense: object|null}>}
 */
export async function validateUserDocuments(userId, token) {
      // Convert to number and validate
  const numUserId = Number(userId)
  console.log('[validateUserDocuments] received userId:', userId, 'type:', typeof userId, 'numUserId:', numUserId, 'isNaN:', isNaN(numUserId))

  // Check for various forms of invalid userId
  if (
    userId == null ||
    userId === '' ||
    userId === 'undefined' ||
    String(userId).toLowerCase() === 'undefined' ||
    isNaN(numUserId) ||
    numUserId <= 0
  ) {
    console.error('[validateUserDocuments] Invalid userId detected. Returning error state.', { userId, numUserId })
    return {
      hasAllDocuments: false,
      missingDocs: ['Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.'],
      citizenInfo: null,
      driverLicense: null,
      error: new Error('INVALID_USER_ID')
    }
  }
  try {
    const [citizenRes, licenseRes] = await Promise.all([
      userApi.getCitizenInfo(token),
      userApi.getDriverLicense(token),
    ])

    const citizenInfo = citizenRes.data
    const driverLicense = licenseRes.data
    const missingDocs = []

    console.log('[validateUserDocuments] citizenInfo:', citizenInfo)
    console.log('[validateUserDocuments] driverLicense:', driverLicense)

    // Check if citizen info exists and is approved
    // Status values: "Đã xác nhận" (Approved), "Chờ xác thực" (Pending), "Từ chối" (Rejected)
    const citizenApproved = citizenInfo?.status === 'Đã xác nhận' || citizenInfo?.status === 'Approved'
    if (!citizenInfo || !citizenApproved) {
      missingDocs.push('Hồ sơ CCCD/CMND')
      console.log('[validateUserDocuments] Citizen info missing/not approved. Status:', citizenInfo?.status)
    }

    // Check if driver license exists and is approved
    const licenseApproved = driverLicense?.status === 'Đã xác nhận' || driverLicense?.status === 'Approved'
    if (!driverLicense || !licenseApproved) {
      missingDocs.push('Giấy phép lái xe')
      console.log('[validateUserDocuments] Driver license missing/not approved. Status:', driverLicense?.status)
    }

    return {
      hasAllDocuments: missingDocs.length === 0,
      missingDocs,
      citizenInfo: citizenInfo || null,
      driverLicense: driverLicense || null,
    }
  } catch (error) {
    console.error('Error validating documents:', error)
    return {
      hasAllDocuments: false,
      missingDocs: ['Không thể kiểm tra tài liệu'],
      citizenInfo: null,
      driverLicense: null,
      error,
    }
  }
}

/**
 * Get user's document status summary
 * @param {number} userId - User ID
 * @param {string} token - JWT token
 * @returns {Promise<{status: string, message: string}>}
 */
export async function getDocumentStatus(userId, token) {
  try {
    const validation = await validateUserDocuments(userId, token)

    if (validation.hasAllDocuments) {
      return {
        status: 'approved',
        message: 'Tất cả giấy tờ đã được xác minh',
      }
    }

    if (validation.missingDocs.length > 0) {
      return {
        status: 'incomplete',
        message: `Thiếu tài liệu: ${validation.missingDocs.join(', ')}`,
        missingDocs: validation.missingDocs,
      }
    }

    return {
      status: 'pending',
      message: 'Giấy tờ đang chờ xác minh',
    }
  } catch (error) {
    return {
      status: 'error',
      message: 'Lỗi kiểm tra tài liệu',
      error,
    }
  }
}

export default { validateUserDocuments, getDocumentStatus }
