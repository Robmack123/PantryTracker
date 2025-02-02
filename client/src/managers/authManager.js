const _apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/auth";

export const login = (email, password) => {
  return fetch(_apiUrl + "/login", {
    method: "POST",
    credentials: "include",
    headers: {
      Authorization: `Basic ${btoa(`${email}:${password}`)}`,
    },
  }).then((res) => {
    if (res.status !== 200) {
      return Promise.resolve(null);
    } else {
      // Instead of calling tryGetLoggedInUser(), return the JSON from the login response.
      return res.json();
    }
  });
};

export const logout = () => {
  return fetch(_apiUrl + "/logout");
};

export const tryGetLoggedInUser = () => {
  // Normalize the current pathname
  const currentPath = window.location.pathname
    .replace(/\/+$/, "")
    .toLowerCase();

  // If we're on the login or register page, just resolve to null
  if (currentPath === "/login" || currentPath === "/register") {
    console.log("Skipping /me call on", currentPath);
    return Promise.resolve(null);
  }

  // Otherwise, call the /me endpoint.
  return fetch(_apiUrl + "/me", { credentials: "include" })
    .then((res) => {
      // If we get a 401, just resolve to null (do not redirect here)
      if (res.status === 401) {
        return null;
      }
      return res.json();
    })
    .catch((error) => {
      console.error("Error fetching user profile:", error);
      return null;
    });
};

export const register = (userProfile) => {
  userProfile.password = btoa(userProfile.password);
  return fetch(_apiUrl + "/register", {
    credentials: "include",
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(userProfile),
  }).then((res) => {
    if (!res.ok) {
      return res.json().then((error) => {
        throw new Error(error.message || "Registration failed.");
      });
    }
    // Instead of calling tryGetLoggedInUser(), return a dummy success object.
    return {};
  });
};
