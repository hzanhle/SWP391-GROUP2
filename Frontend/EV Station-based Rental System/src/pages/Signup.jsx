import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import Footer from '../components/Footer'
import CTA from '../components/CTA'
import api from '../api/client'

export default function Signup() {
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    if (submitting) return
    setSubmitting(true)

    const form = e.currentTarget
    const payload = {
      userName: form.username.value.trim(),
      email: form.email.value.trim(),
      phoneNumber: form.phone.value.trim(),
      password: form.password.value,
    }

    try {
      await api.registerUser(payload)
      alert(`Signed up as ${payload.userName}`)
      window.location.hash = ''
    } catch (err) {
      const msg = err?.message || 'Registration failed'
      alert(msg)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-figma-layer="Signup Page">
      <Navbar />
      <main>
        <section id="signup" className="section" aria-labelledby="signup-title">
          <div className="container">
            <div className="section-header">
              <h1 id="signup-title" className="section-title">Create your account</h1>
              <p className="section-subtitle">Join to book EVs and charging stations with ease.</p>
            </div>

            <div className="card">
              <form className="card-body" onSubmit={handleSubmit} noValidate>
                <div className="field">
                  <label htmlFor="username" className="label">Username</label>
                  <input id="username" name="username" className="input" type="text" placeholder="yourname" autoComplete="username" required />
                </div>

                <div className="field">
                  <label htmlFor="email" className="label">Email</label>
                  <input id="email" name="email" className="input" type="email" placeholder="you@example.com" autoComplete="email" required />
                </div>

                <div className="field">
                  <label htmlFor="phone" className="label">Phone</label>
                  <input id="phone" name="phone" className="input" type="tel" placeholder="(+84) 901-234-567" autoComplete="tel" pattern="^[0-9+()\-\s]{7,}$" required />
                </div>

                <div className="field">
                  <label htmlFor="password" className="label">Password</label>
                  <input id="password" name="password" className="input" type="password" placeholder="••••••••" autoComplete="new-password" minLength={6} required />
                </div>

                <div className="row-between">
                  <a className="nav-link" href="#">Back to Home</a>
                  <CTA as="button" type="submit" disabled={submitting} aria-busy={submitting}>
                    {submitting ? 'Signing up…' : 'Sign up'}
                  </CTA>
                </div>
              </form>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </div>
  )
}
