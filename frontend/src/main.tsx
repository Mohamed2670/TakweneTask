import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './style.css'

function App() {
  return <main><h1>Takwene Track Management</h1><p>Project foundation is ready. Login and track views will follow in their planned milestones.</p></main>
}

createRoot(document.getElementById('root')!).render(<StrictMode><App /></StrictMode>)
