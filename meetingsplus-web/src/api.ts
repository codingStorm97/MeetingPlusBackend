import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'
export const NOTIFICATIONS_WS_URL =
  import.meta.env.VITE_NOTIFICATIONS_WS_URL ?? '/api/notifications/ws'
export const KAFKA_BOOTSTRAP_SERVERS =
  import.meta.env.VITE_KAFKA_BOOTSTRAP_SERVERS ?? 'localhost:9050'
export const KAFKA_TOPIC_FILE_UPLOADED =
  import.meta.env.VITE_KAFKA_TOPIC_FILE_UPLOADED ?? 'file-uploaded'
export const KAFKA_TOPIC_JOB_STATUS_CHANGED =
  import.meta.env.VITE_KAFKA_TOPIC_JOB_STATUS_CHANGED ?? 'job-status-changed'

export type AuthResponse = {
  userId: string
  email: string
  token: string
}

export type AuthRequest = {
  email: string
  password: string
}

export type Appointment = {
  id: string
  userId: string
  title: string
  description: string
  startTime: string
  endTime: string
  profile: string
  status: number
  source: number
  createdAt: string
}

export type UserFile = {
  fileId: string
  fileName: string
  status: string
  relativePath: string
  downloadUrl: string
}

export type UserFilesResponse = {
  userName: string
  directory: string
  files: UserFile[]
}

export type CleanFolderResponse = {
  userId: string
  userName: string
  deletedFileCount: number
  deletedJobRecordCount: number
  deletedFileHistoryRecordCount: number
}

export type Conversation = {
  conversationId: string
  conversationName: string
  createdAt: string
}

export type ConversationDetails = {
  conversationId: string
  title: string
  createdAt: string
  questions: ConversationQuestionItem[]
}

export type ConversationQuestionItem = {
  questionId: string
  question: string
  answer: string | null
  createdAt: string
  answeredTime: string | null
}

export type ConversationAnswer = {
  conversationId: string
  questionId: string
  question: string
  answer: string
}

export type SemanticSearchResult = {
  score: number
  fileName: string
  relativePath: string
  chunkText: string
  textBlock: string
}

export type SemanticSearchResponse = {
  query: string
  results: SemanticSearchResult[]
}

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

export async function login(request: AuthRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/auth/login', request)
  return response.data
}

export async function register(request: AuthRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/auth/register', request)
  return response.data
}

export async function getAppointments(token: string): Promise<Appointment[]> {
  const response = await apiClient.get<Appointment[]>('/appointments', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function uploadTextFile(token: string, file: File): Promise<{ message: string; path: string }> {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<{
    jobId: string
    userId: string
    userName: string
    fileName: string
    message: string
    path: string
    timeUtc: string
  }>('/files/upload', formData, {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'multipart/form-data',
    },
  })
  return response.data
}

export async function getUserFiles(token: string): Promise<UserFilesResponse> {
  const response = await apiClient.get<UserFilesResponse>('/files', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function downloadUserFile(token: string, fileName: string): Promise<Blob> {
  const encoded = encodeURIComponent(fileName)
  const response = await apiClient.get<Blob>(`/files/download/${encoded}`, {
    headers: { Authorization: `Bearer ${token}` },
    responseType: 'blob',
  })
  return response.data
}

export async function cleanUserFolder(token: string): Promise<CleanFolderResponse> {
  const response = await apiClient.delete<CleanFolderResponse>('/files/clean-folder', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function getConversations(token: string): Promise<Conversation[]> {
  const response = await apiClient.get<Conversation[]>('/conversation', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function createConversation(token: string, title?: string): Promise<Conversation> {
  const response = await apiClient.post<{
    conversationId: string
    title: string
    createdAt: string
    questions: ConversationQuestionItem[]
  }>(
    '/conversation',
    { title },
    { headers: { Authorization: `Bearer ${token}` } },
  )
  return {
    conversationId: response.data.conversationId,
    conversationName: response.data.title,
    createdAt: response.data.createdAt,
  }
}

export async function askConversation(
  token: string,
  request: { conversationId: string; question: string },
): Promise<ConversationAnswer> {
  const response = await apiClient.post<ConversationAnswer>('/conversation/chat', request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function getConversationById(token: string, conversationId: string): Promise<ConversationDetails> {
  const response = await apiClient.get<ConversationDetails>(`/conversation/${conversationId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

export async function semanticSearch(
  token: string,
  request: { text: string; topK?: number },
): Promise<SemanticSearchResponse> {
  const response = await apiClient.post<SemanticSearchResponse>('/search/semantic', request, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}
