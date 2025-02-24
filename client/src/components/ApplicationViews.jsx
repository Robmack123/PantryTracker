import { Route, Routes } from "react-router-dom";
import { AuthorizedRoute } from "./auth/AuthorizedRoute";
import Login from "./auth/Login";
import Register from "./auth/Register";
import ManageHousehold from "./household/ManageHousehold";
import { PantryItems } from "./pantry/PantryItems";
import { HomePage } from "./homepage/Homepage";
import HouseholdSelection from "./household/HouseholdSelection"; // Import the new component

export default function ApplicationViews({ loggedInUser, setLoggedInUser }) {
  return (
    <Routes>
      <Route path="/">
        <Route
          index
          element={
            <AuthorizedRoute loggedInUser={loggedInUser}>
              <HomePage />
            </AuthorizedRoute>
          }
        />
        <Route
          path="login"
          element={<Login setLoggedInUser={setLoggedInUser} />}
        />
        <Route
          path="register"
          element={<Register setLoggedInUser={setLoggedInUser} />}
        />
        <Route
          path="household/manage"
          element={
            <AuthorizedRoute loggedInUser={loggedInUser}>
              <ManageHousehold />
            </AuthorizedRoute>
          }
        />
        <Route
          path="household-selection"
          element={
            <AuthorizedRoute loggedInUser={loggedInUser}>
              <HouseholdSelection
                loggedInUser={loggedInUser}
                setLoggedInUser={setLoggedInUser}
              />
            </AuthorizedRoute>
          }
        />
        <Route
          path="/pantry"
          element={
            <AuthorizedRoute loggedInUser={loggedInUser}>
              <PantryItems />
            </AuthorizedRoute>
          }
        />
      </Route>
      <Route path="*" element={<p>Whoops, nothing here...</p>} />
    </Routes>
  );
}
