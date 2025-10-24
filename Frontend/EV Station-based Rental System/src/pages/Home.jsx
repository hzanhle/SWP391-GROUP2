import Hero from "../components/Hero";
import HowItWorks from "../components/HowItWorks";
import FeaturedModels from "../components/FeaturedModels";
import Banner from "../components/Banner";
import ChooseUs from "../components/ChooseUs";
import Testimonials from "../components/Testimonials";
import FeaturedStations from "../components/FeaturedStations";
import Footer from "../components/Footer";

function Home() {
  return (
    <>
      <Hero />
      <FeaturedModels />
      <HowItWorks />
      <Banner />
      <ChooseUs />
      <FeaturedStations />
      <Testimonials />
      <Footer />
    </>
  );
}

export default Home;
