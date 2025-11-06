function ChooseUs() {
  return (
    <>
      <section className="choose-section">
        <div className="container">
          <div className="choose-container">
            <div className="image-placeholder" aria-hidden="true">Why Choose Us</div>
            <div className="text-container">
              <div className="text-container__left">
                <h4>Why Choose Us</h4>
                <h2>Best EV rental experience</h2>
                <p>
                  Discover the best deals and service you'll ever find with our premium EV rental options. We're dedicated to providing you with the best value for your money, so you can enjoy top-quality electric vehicles without breaking the bank. Our service is designed to give you the ultimate EV experience, so don't miss out on your chance to save while going green.
                </p>
                <a href="#home">
                  Find Details &nbsp;
                  <i className="fa-solid fa-angle-right" aria-hidden="true"></i>
                </a>
              </div>
              <div className="text-container__right">
                <div className="text-container__right__box">
                  <div className="feature-icon" aria-hidden="true">🚗</div>
                  <div className="text-container__right__box__text">
                    <h4>Long Distance Drive</h4>
                    <p>
                      Take your driving experience to the next level with our high-performance electric vehicles for extended adventures.
                    </p>
                  </div>
                </div>
                <div className="text-container__right__box">
                  <div className="feature-icon" aria-hidden="true">💰</div>
                  <div className="text-container__right__box__text">
                    <h4>All Inclusive Pricing</h4>
                    <p>
                      Get everything you need in one convenient, transparent price with our all-inclusive pricing policy.
                    </p>
                  </div>
                </div>
                <div className="text-container__right__box">
                  <div className="feature-icon" aria-hidden="true">✓</div>
                  <div className="text-container__right__box__text">
                    <h4>No Hidden Charges</h4>
                    <p>
                      Enjoy peace of mind with our no hidden charges policy. We believe in transparent and honest pricing.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export default ChooseUs;
