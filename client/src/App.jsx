import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import { Spinner } from "reactstrap";
import NavBar from "./components/NavBar";
import ApplicationViews from "./components/ApplicationViews";
import { fetchProtectedData } from "./managers/authManager"; // This should be your new function that uses the JWT

function App() {
  const [loggedInUser, setLoggedInUser] = useState();
  const location = useLocation();

  useEffect(() => {
    // If we're on the login or register page, do not attempt to fetch protected user data.
    if (location.pathname === "/login" || location.pathname === "/register") {
      setLoggedInUser(null);
      return;
    }
    // Otherwise, fetch the user data using your token-based API call.
    fetchProtectedData().then((user) => {
      setLoggedInUser(user);
    });
  }, [location.pathname]);

  // Wait for a definite logged-in state before rendering.
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
