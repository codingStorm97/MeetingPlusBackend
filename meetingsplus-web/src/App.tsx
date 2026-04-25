import {
  Alert,
  AppBar,
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Container,
  Divider,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Paper,
  Stack,
  Tab,
  Tabs,
  TextField,
  Toolbar,
  Typography,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import AttachFileIcon from '@mui/icons-material/AttachFile'
import ForumOutlinedIcon from '@mui/icons-material/ForumOutlined'
import LogoutIcon from '@mui/icons-material/Logout'
import SendRoundedIcon from '@mui/icons-material/SendRounded'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import { AxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import {
  askConversation,
  cleanUserFolder,
  createConversation,
  getConversationById,
  getConversations,
  NOTIFICATIONS_WS_URL,
  downloadUserFile,
  getUserFiles,
  login,
  register,
  semanticSearch,
  uploadTextFile,
} from './api'
import type { FormEvent } from 'react'
import type { AuthResponse, Conversation, SemanticSearchResult, UserFile } from './api'

type FileUploadedEvent = {
  jobId: string
  userId: string
  userName: string
  fileName: string
  relativePath: string
  timeUtc: string
}

type JobStatusChangedEvent = {
  jobId: string
  userId: string
  fileName: string
  status: string
  timeUtc: string
  error?: string | null
}

type AuthMode = 'login' | 'register'

type AuthFormState = {
  email: string
  password: string
}

function getStoredAuth(): AuthResponse | null {
  const raw = localStorage.getItem('auth')
  return raw ? (JSON.parse(raw) as AuthResponse) : null
}

function App() {
  const [auth, setAuth] = useState<AuthResponse | null>(getStoredAuth())

  const handleAuthenticated = (response: AuthResponse) => {
    localStorage.setItem('auth', JSON.stringify(response))
    setAuth(response)
  }

  const handleLogout = () => {
    localStorage.removeItem('auth')
    setAuth(null)
  }

  return (
    <Routes>
      <Route
        path="/login"
        element={
          auth ? (
            <Navigate to="/dashboard" replace />
          ) : (
            <AuthPage onAuthenticated={handleAuthenticated} />
          )
        }
      />
      <Route
        path="/dashboard"
        element={
          auth ? (
            <DashboardPage auth={auth} onLogout={handleLogout} />
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      <Route
        path="*"
        element={<Navigate to={auth ? '/dashboard' : '/login'} replace />}
      />
    </Routes>
  )
}

function AuthPage({
  onAuthenticated,
}: {
  onAuthenticated: (response: AuthResponse) => void
}) {
  const [mode, setMode] = useState<AuthMode>('login')
  const [form, setForm] = useState<AuthFormState>({ email: '', password: '' })
  const [error, setError] = useState<string>('')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const actionLabel = useMemo(
    () => (mode === 'login' ? 'Sign In' : 'Create Account'),
    [mode],
  )

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response =
        mode === 'login' ? await login(form) : await register(form)
      onAuthenticated(response)
      navigate('/dashboard', { replace: true })
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setError(apiError.response?.data?.message ?? 'Request failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Container maxWidth="sm" sx={{ mt: 10 }}>
      <Card elevation={4}>
        <CardContent>
          <Typography variant="h4" sx={{ fontWeight: 700 }} gutterBottom>
            MeetingsPlus
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
            Use your account to manage meetings and appointments.
          </Typography>

          <Tabs
            value={mode}
            onChange={(_, value: AuthMode) => setMode(value)}
            sx={{ mb: 3 }}
          >
            <Tab value="login" label="Login" />
            <Tab value="register" label="Register" />
          </Tabs>

          <Box component="form" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <TextField
                label="Email"
                type="email"
                required
                value={form.email}
                onChange={(event) =>
                  setForm((prev) => ({ ...prev, email: event.target.value }))
                }
              />
              <TextField
                label="Password"
                type="password"
                required
                value={form.password}
                onChange={(event) =>
                  setForm((prev) => ({ ...prev, password: event.target.value }))
                }
              />
              {error ? <Alert severity="error">{error}</Alert> : null}
              <Button
                type="submit"
                variant="contained"
                size="large"
                disabled={loading}
              >
                {loading ? <CircularProgress size={22} color="inherit" /> : actionLabel}
              </Button>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

function DashboardPage({
  auth,
  onLogout,
}: {
  auth: AuthResponse
  onLogout: () => void
}) {
  const [chatInput, setChatInput] = useState('')
  const [chatMessages, setChatMessages] = useState<string[]>(['Welcome! This is your assistant chat box.'])
  const [conversations, setConversations] = useState<Conversation[]>([])
  const [activeConversationId, setActiveConversationId] = useState<string>('')
  const [chatLoading, setChatLoading] = useState(false)

  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [uploadLoading, setUploadLoading] = useState(false)
  const [cleanLoading, setCleanLoading] = useState(false)
  const [uploadMessage, setUploadMessage] = useState('')
  const [uploadError, setUploadError] = useState('')
  const [userDirectory, setUserDirectory] = useState('')
  const [userFiles, setUserFiles] = useState<UserFile[]>([])
  const [filesLoading, setFilesLoading] = useState(true)
  const [filesError, setFilesError] = useState('')
  const [notification, setNotification] = useState('')
  const [semanticQuery, setSemanticQuery] = useState('')
  const [semanticLoading, setSemanticLoading] = useState(false)
  const [semanticResults, setSemanticResults] = useState<SemanticSearchResult[]>([])
  const [semanticError, setSemanticError] = useState('')

  const loadFiles = async () => {
    setFilesLoading(true)
    setFilesError('')
    try {
      const response = await getUserFiles(auth.token)
      setUserDirectory(response.directory)
      setUserFiles(response.files)
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setFilesError(apiError.response?.data?.message ?? 'Unable to load files')
    } finally {
      setFilesLoading(false)
    }
  }

  const loadConversations = async () => {
    try {
      const response = await getConversations(auth.token)
      setConversations(response)
      if (!activeConversationId && response.length > 0) {
        setActiveConversationId(response[0].conversationId)
        try {
          const thread = await getConversationById(auth.token, response[0].conversationId)
          const messages = thread.questions.flatMap((q) =>
            q.answer ? [q.question, q.answer] : [q.question],
          )
          setChatMessages(messages.length > 0 ? messages : ['Welcome! This is your assistant chat box.'])
        } catch {
          setChatMessages(['Welcome! This is your assistant chat box.'])
        }
      }
    } catch {
      // Keep chat usable even if conversation list fetch fails.
    }
  }

  useEffect(() => {
    void loadFiles()
    void loadConversations()
  }, [auth.token])

  useEffect(() => {
    const wsBase = NOTIFICATIONS_WS_URL.startsWith('http')
      ? NOTIFICATIONS_WS_URL.replace(/^http/i, 'ws')
      : `${window.location.protocol === 'https:' ? 'wss' : 'ws'}://${window.location.host}${NOTIFICATIONS_WS_URL}`
    const wsUrl = `${wsBase}?access_token=${encodeURIComponent(auth.token)}`
    const socket = new WebSocket(wsUrl)

    socket.onmessage = (event) => {
      try {
        const parsed = JSON.parse(event.data) as {
          eventType: 'fileUploaded' | 'jobStatusChanged'
          data: FileUploadedEvent | JobStatusChangedEvent
        }

        if (parsed.eventType === 'fileUploaded') {
          const uploaded = parsed.data as FileUploadedEvent
          if (uploaded.userId !== auth.userId) {
            return
          }
          setNotification(`File uploaded: ${uploaded.fileName}. Processing started.`)
          void loadFiles()
          return
        }

        const changed = parsed.data as JobStatusChangedEvent
        if (changed.userId !== auth.userId) {
          return
        }

        if (changed.status === 'UploadedAndVectorized') {
          setNotification(`${changed.fileName}: ready to search`)
        } else if (changed.status === 'Failed') {
          setNotification(`${changed.fileName}: failed (${changed.error ?? 'unknown error'})`)
        } else {
          setNotification(`${changed.fileName}: ${changed.status}`)
        }

        void loadFiles()
      } catch {
        setNotification('Notification parsing failed')
      }
    }

    socket.onerror = () => {
      setNotification('Notification socket disconnected')
    }

    return () => {
      socket.close()
    }
  }, [auth.userId, auth.token])

  const handleSendMessage = async () => {
    const trimmed = chatInput.trim()
    if (!trimmed) {
      return
    }
    setChatMessages((prev) => [...prev, trimmed])
    setChatInput('')

    setChatLoading(true)
    try {
      let conversationId = activeConversationId
      if (!conversationId) {
        const created = await createConversation(auth.token, 'New conversation')
        conversationId = created.conversationId
        setActiveConversationId(conversationId)
        setConversations((prev) => [created, ...prev])
      }

      const response = await askConversation(auth.token, {
        conversationId,
        question: trimmed,
      })

      setChatMessages((prev) => [...prev, response.answer])
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setChatMessages((prev) => [
        ...prev,
        apiError.response?.data?.message ?? 'Conversation request failed',
      ])
    } finally {
      setChatLoading(false)
    }
  }

  const handleFileUpload = async () => {
    if (!selectedFile) {
      setUploadError('Please select a .txt file.')
      return
    }

    if (!selectedFile.name.toLowerCase().endsWith('.txt')) {
      setUploadError('Only .txt files are allowed.')
      return
    }

    setUploadLoading(true)
    setUploadError('')
    setUploadMessage('')

    try {
      const result = await uploadTextFile(auth.token, selectedFile)
      setUploadMessage(`${result.message} (${result.path})`)
      setSelectedFile(null)
      await loadFiles()
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setUploadError(apiError.response?.data?.message ?? 'File upload failed')
    } finally {
      setUploadLoading(false)
    }
  }

  const handleDownload = async (fileName: string) => {
    try {
      const blob = await downloadUserFile(auth.token, fileName)
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = fileName
      document.body.appendChild(anchor)
      anchor.click()
      anchor.remove()
      URL.revokeObjectURL(url)
    } catch {
      setFilesError('Unable to download file')
    }
  }

  const handleCleanFolder = async () => {
    setCleanLoading(true)
    setFilesError('')
    setNotification('')

    try {
      const result = await cleanUserFolder(auth.token)
      setNotification(
        `Folder cleaned: ${result.deletedFileCount} files, ${result.deletedJobRecordCount} jobs, ${result.deletedFileHistoryRecordCount} history records removed.`,
      )
      await loadFiles()
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setFilesError(apiError.response?.data?.message ?? 'Unable to clean folder')
    } finally {
      setCleanLoading(false)
    }
  }

  const handleSemanticSearch = async () => {
    const text = semanticQuery.trim()
    if (!text) {
      setSemanticError('Please enter text to search.')
      return
    }

    setSemanticLoading(true)
    setSemanticError('')
    try {
      const response = await semanticSearch(auth.token, { text, topK: 5 })
      setSemanticResults(response.results)
      if (response.results.length === 0) {
        setSemanticError('No semantic matches found.')
      }
    } catch (error) {
      const apiError = error as AxiosError<{ message?: string }>
      setSemanticError(apiError.response?.data?.message ?? 'Semantic search failed')
      setSemanticResults([])
    } finally {
      setSemanticLoading(false)
    }
  }

  return (
    <Box sx={{ height: '100vh', display: 'flex', bgcolor: '#f7f7f8' }}>
      <Box
        sx={{
          width: { xs: 0, md: 260 },
          display: { xs: 'none', md: 'flex' },
          flexDirection: 'column',
          bgcolor: '#171717',
          color: '#fff',
          p: 1.5,
          gap: 1,
        }}
      >
        <Button
          variant="outlined"
          fullWidth
          onClick={async () => {
            try {
              const created = await createConversation(auth.token, `Chat ${conversations.length + 1}`)
              setConversations((prev) => [created, ...prev])
              setActiveConversationId(created.conversationId)
              setChatMessages(['Welcome! This is your assistant chat box.'])
            } catch (error) {
              const apiError = error as AxiosError<{ message?: string }>
              setNotification(apiError.response?.data?.message ?? 'Unable to create conversation')
            }
          }}
          sx={{ justifyContent: 'flex-start', color: '#fff', borderColor: '#4a4a4a' }}
        >
          <AddIcon sx={{ mr: 1, fontSize: 18 }} />
          New chat
        </Button>
        <List dense sx={{ flex: 1, overflowY: 'auto' }}>
          {conversations.map((item) => (
            <ListItem key={item.conversationId} disablePadding>
              <ListItemButton
                selected={activeConversationId === item.conversationId}
                onClick={() => {
                  setActiveConversationId(item.conversationId)
                  void (async () => {
                    try {
                      const thread = await getConversationById(auth.token, item.conversationId)
                      const messages = thread.questions.flatMap((q) =>
                        q.answer ? [q.question, q.answer] : [q.question],
                      )
                      setChatMessages(
                        messages.length > 0 ? messages : ['Welcome! This is your assistant chat box.'],
                      )
                    } catch {
                      setChatMessages(['Welcome! This is your assistant chat box.'])
                    }
                  })()
                }}
                sx={{ borderRadius: 1 }}
              >
                <ListItemIcon sx={{ minWidth: 28, color: '#bdbdbd' }}>
                  <ForumOutlinedIcon sx={{ fontSize: 18 }} />
                </ListItemIcon>
                <ListItemText
                  primary={item.conversationName}
                  sx={{ '& .MuiTypography-root': { fontSize: 13, color: '#e0e0e0' } }}
                />
              </ListItemButton>
            </ListItem>
          ))}
          {conversations.length === 0 ? (
            <ListItem>
              <ListItemText
                primary="No conversations"
                sx={{ '& .MuiTypography-root': { fontSize: 13, color: '#9e9e9e' } }}
              />
            </ListItem>
          ) : null}
        </List>
        <Button
          variant="text"
          onClick={onLogout}
          sx={{ justifyContent: 'flex-start', color: '#fff' }}
        >
          <LogoutIcon sx={{ mr: 1, fontSize: 18 }} />
          Logout
        </Button>
      </Box>

      <Box sx={{ flex: 1, display: 'flex' }}>
        <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
          <AppBar position="static" elevation={0} sx={{ bgcolor: '#fff', color: '#111' }}>
            <Toolbar sx={{ justifyContent: 'space-between' }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                MeetingsPlus Chat
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {auth.email}
              </Typography>
            </Toolbar>
          </AppBar>

          <Box sx={{ flex: 1, overflowY: 'auto', p: { xs: 1.5, md: 3 } }}>
            {notification ? <Alert severity="info" sx={{ mb: 2 }}>{notification}</Alert> : null}
            <Stack spacing={2}>
              {chatMessages.map((message, index) => {
                const isUser = index % 2 !== 0
                return (
                  <Stack
                    key={`${message}-${index}`}
                    direction="row"
                    spacing={1.5}
                    sx={{ justifyContent: isUser ? 'flex-end' : 'flex-start' }}
                  >
                    {!isUser ? (
                      <Avatar sx={{ width: 28, height: 28, bgcolor: '#10a37f', fontSize: 12 }}>
                        AI
                      </Avatar>
                    ) : null}
                    <Paper
                      elevation={0}
                      sx={{
                        p: 1.5,
                        borderRadius: 2,
                        maxWidth: '75%',
                        bgcolor: isUser ? '#e7f0ff' : '#fff',
                        border: '1px solid #e5e5e5',
                      }}
                    >
                      <Typography variant="body2">{message}</Typography>
                    </Paper>
                    {isUser ? (
                      <Avatar sx={{ width: 28, height: 28, bgcolor: '#6366f1', fontSize: 12 }}>
                        U
                      </Avatar>
                    ) : null}
                  </Stack>
                )
              })}
            </Stack>
          </Box>

          <Box sx={{ p: 2, borderTop: '1px solid #e5e5e5', bgcolor: '#fff' }}>
            <Paper
              variant="outlined"
              sx={{ p: 1, borderRadius: 3, display: 'flex', alignItems: 'flex-end', gap: 1 }}
            >
              <IconButton size="small" sx={{ width: 34, height: 34 }}>
                <AttachFileIcon sx={{ fontSize: 18 }} />
              </IconButton>
              <TextField
                fullWidth
                multiline
                maxRows={4}
                size="small"
                variant="outlined"
                placeholder="Message MeetingsPlus..."
                value={chatInput}
                onChange={(event) => setChatInput(event.target.value)}
              />
              <Button
                variant="contained"
                onClick={() => void handleSendMessage()}
                disabled={chatLoading}
                sx={{ minWidth: 36, width: 36, height: 36, borderRadius: '50%', p: 0 }}
              >
                <SendRoundedIcon sx={{ fontSize: 18 }} />
              </Button>
            </Paper>
          </Box>
        </Box>

        <Box
          sx={{
            width: { xs: 0, lg: 320 },
            display: { xs: 'none', lg: 'block' },
            borderLeft: '1px solid #e5e5e5',
            bgcolor: '#fbfbfc',
            p: 2,
            overflowY: 'auto',
          }}
        >
          <Stack spacing={2}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1" gutterBottom>
                  Upload
                </Typography>
                <Divider sx={{ mb: 1.5 }} />
                <Stack spacing={1.25}>
                  <Button component="label" variant="outlined" size="small">
                    <UploadFileIcon sx={{ mr: 0.5, fontSize: 16 }} />
                    Choose .txt
                    <input
                      type="file"
                      hidden
                      accept=".txt,text/plain"
                      onChange={(event) => setSelectedFile(event.target.files?.[0] ?? null)}
                    />
                  </Button>
                  {selectedFile ? <Typography variant="caption">{selectedFile.name}</Typography> : null}
                  {uploadError ? <Alert severity="error">{uploadError}</Alert> : null}
                  {uploadMessage ? <Alert severity="success">{uploadMessage}</Alert> : null}
                  <Button
                    variant="contained"
                    size="small"
                    onClick={handleFileUpload}
                    disabled={uploadLoading}
                  >
                    {uploadLoading ? <CircularProgress size={16} color="inherit" /> : 'Upload'}
                  </Button>
                </Stack>
              </CardContent>
            </Card>

            <Card>
              <CardContent>
                <Typography variant="subtitle1" gutterBottom>
                  Semantic Search
                </Typography>
                <Stack spacing={1.25} sx={{ mb: 2 }}>
                  <TextField
                    size="small"
                    placeholder="Search your embedded documents..."
                    value={semanticQuery}
                    onChange={(event) => setSemanticQuery(event.target.value)}
                  />
                  <Button
                    variant="contained"
                    size="small"
                    onClick={() => void handleSemanticSearch()}
                    disabled={semanticLoading}
                  >
                    {semanticLoading ? <CircularProgress size={16} color="inherit" /> : 'Semantic Search'}
                  </Button>
                  {semanticError ? <Alert severity="info">{semanticError}</Alert> : null}
                  {semanticResults.length > 0 ? (
                    <Stack spacing={1} sx={{ maxHeight: 220, overflowY: 'auto', pr: 0.5 }}>
                      {semanticResults.map((item, index) => (
                        <Paper key={`${item.fileName}-${index}`} variant="outlined" sx={{ p: 1 }}>
                          <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 0.5 }}>
                            Score: {item.score.toFixed(4)}
                          </Typography>
                          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                            {item.textBlock}
                          </Typography>
                        </Paper>
                      ))}
                    </Stack>
                  ) : null}
                </Stack>

                <Divider sx={{ mb: 1.5 }} />
                <Typography variant="subtitle1" gutterBottom>
                  Directory
                </Typography>
                <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
                  {userDirectory || 'Data/<username>'}
                </Typography>
                <Button
                  variant="outlined"
                  color="error"
                  size="small"
                  onClick={handleCleanFolder}
                  disabled={cleanLoading}
                  sx={{ mb: 1.5 }}
                >
                  {cleanLoading ? <CircularProgress size={14} color="inherit" /> : 'Clean'}
                </Button>
                {filesLoading ? <CircularProgress size={18} /> : null}
                {filesError ? <Alert severity="error">{filesError}</Alert> : null}
                {!filesLoading && !filesError ? (
                  <List dense sx={{ maxHeight: 260, overflowY: 'auto', p: 0 }}>
                    {userFiles.length === 0 ? (
                      <ListItem>
                        <ListItemText primary="No files yet." />
                      </ListItem>
                    ) : (
                      userFiles.map((file) => (
                        <ListItem
                          key={file.fileId}
                          divider
                          secondaryAction={
                            <Button size="small" onClick={() => handleDownload(file.fileName)}>
                              Open
                            </Button>
                          }
                        >
                          <ListItemButton onClick={() => handleDownload(file.fileName)}>
                            <ListItemText primary={file.fileName} secondary={file.status} />
                          </ListItemButton>
                        </ListItem>
                      ))
                    )}
                  </List>
                ) : null}
              </CardContent>
            </Card>
          </Stack>
        </Box>
      </Box>
    </Box>
  )
}

export default App
