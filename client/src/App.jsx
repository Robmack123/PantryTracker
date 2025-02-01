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
    // If we're on /login or /register, don't call the /me endpoint.
    if (location.pathname === "/login" || location.pathname === "/register") {
      // Set loggedInUser to null if not authenticated
      setLoggedInUser(null);
      return;
    }
    // For all other pages, call the /me endpoint.
    tryGetLoggedInUser().then((user) => {
      setLoggedInUser(user);
    });
  }, [location.pathname]);

  // Wait to get a definite logged-in state before rendering
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
