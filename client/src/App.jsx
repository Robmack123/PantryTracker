import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import { tryGetLoggedInUser } from "./managers/authManager";
import { Spinner } from "reactstrap";
import NavBar from "./components/NavBar";
import ApplicationViews from "./components/ApplicationViews";

function App() {
  const [loggedInUser, setLoggedInUser] = useState();
  const location = useLocation();

  useEffect(() => {
    // If we're on /login or /register, skip calling /me
    if (location.pathname === "/login" || location.pathname === "/register") {
      setLoggedInUser(null);
      return;
    }
    tryGetLoggedInUser().then((user) => {
      setLoggedInUser(user);
    });
  }, [location.pathname]);

  // Wait for a definite logged-in state before rendering
  if (loggedInUser === undefined) {
    return <Spinner />;
  }

  return (
    <>
      <NavBar loggedInUser={loggedInUser} setLoggedInUser={setLoggedInUser} />
      <ApplicationViews
        loggedInUser={loggedInUser}
        setLoggedInUser={setLoggedInUser}
      />
    </>
  );
}

export default App;
