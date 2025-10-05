import React from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'

const items = [
  { id:'BK-2024-0001', date:'2024-09-12', station:'Central Hub', model:'Tesla Model 3', cost:48, status:'Completed' },
  { id:'BK-2024-0002', date:'2024-09-05', station:'Riverside', model:'Nissan Leaf', cost:32, status:'Completed' },
]

export default function History() {
  return (
    <div data-figma-layer="History Page">
      <Navbar />
      <main>
        <section id="history" className="section" aria-labelledby="history-title">
          <div className="container">
            <div className="section-header">
              <h1 id="history-title" className="section-title">Lịch sử thuê</h1>
              <p className="section-subtitle">Xem lại chuyến thuê và chi phí.</p>
            </div>

            <div className="card">
              <div className="card-body">
                <ul className="history-list" role="list">
                  {items.map((i)=> (
                    <li key={i.id} className="history-item">
                      <div className="row-between">
                        <div>
                          <h3 className="card-title">{i.id}</h3>
                          <p className="card-subtext">{i.date} • {i.station} • {i.model}</p>
                        </div>
                        <div className="row">
                          <span className="badge green">{i.status}</span>
                          <span className="badge gray">${i.cost}</span>
                          <CTA as="a" href="#booking" variant="secondary">Xem</CTA>
                        </div>
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            </div>

          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
