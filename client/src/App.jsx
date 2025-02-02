import { useEffect, useState } from "react";
import "./App.css";
import NavBar from "./components/NavBar";
import ApplicationViews from "./components/ApplicationViews";
import { fetchProtectedData } from "./managers/authManager";

function App() {
  const [loggedInUser, setLoggedInUser] = useState();

  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (token) {
      fetchProtectedData().then((user) => {
        setLoggedInUser(user);
      });
    } else {
      setLoggedInUser(null);
    }
  }, []);

  if (loggedInUser === undefined) {
    return <div>Loading...</div>;
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
