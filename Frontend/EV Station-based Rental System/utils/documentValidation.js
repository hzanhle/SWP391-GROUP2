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
      if (userId == null || userId === '' || userId === 'undefined') {
    console.warn('validateUserDocuments: missing userId → skip API calls')
    return {
      hasAllDocuments: false,
      missingDocs: ['Thiếu userId / chưa đăng nhập'],
      citizenInfo: null,
      driverLicense: null,
      error: new Error('NO_USER_ID')
    }
  }
  try {
    const [citizenRes, licenseRes] = await Promise.all([
      userApi.getCitizenInfo(userId, token),
      userApi.getDriverLicense(userId, token),
    ])

    const citizenInfo = citizenRes.data
    const driverLicense = licenseRes.data
    const missingDocs = []

    // Check if citizen info exists and is approved
    if (!citizenInfo || citizenInfo.status !== 'Approved') {
      missingDocs.push('Hồ sơ CCCD/CMND')
    }

    // Check if driver license exists and is approved
    if (!driverLicense || driverLicense.status !== 'Approved') {
      missingDocs.push('Giấy phép lái xe')
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
