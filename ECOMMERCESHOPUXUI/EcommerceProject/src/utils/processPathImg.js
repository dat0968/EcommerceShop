import { getEndpoint } from '@/utils/axiosClient'

export default function pathReplaceImg(pathSource = getEndpoint(), pathFolder = '', imgName = '') {
  if (!pathSource) return ''
  if (!imgName || imgName.trim().length === 0) return ''
  let base = pathSource.replace('api', '')
  if (!pathFolder) {
    return base + imgName
  }
  // Đảm bảo có dấu / giữa folder và imgName
  if (!pathFolder.endsWith('/')) pathFolder += '/'
  return base + pathFolder + imgName
}
