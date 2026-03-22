import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { Shell } from '@/components/layout/Shell'
import { Dashboard } from '@/pages/Dashboard/Dashboard'
import { Levers } from '@/pages/Levers/Levers'
import { Adapters } from '@/pages/Adapters/Adapters'
import { Programs } from '@/pages/Programs/Programs'
import { ProgramEditor } from '@/pages/Programs/ProgramEditor'
import { History } from '@/pages/History/History'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Shell />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/levers" element={<Levers />} />
          <Route path="/adapters" element={<Adapters />} />
          <Route path="/programs" element={<Programs />} />
          <Route path="/history" element={<History />} />
        </Route>
        <Route path="/programs/new" element={<ProgramEditor />} />
        <Route path="/programs/:id" element={<ProgramEditor />} />
      </Routes>
    </BrowserRouter>
  )
}
