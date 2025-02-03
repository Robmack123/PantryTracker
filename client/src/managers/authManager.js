// managers/authManager.js
const _apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/auth";

// Login: return the JWT and user info
export const login = (email, password) => {
  return fetch(_apiUrl + "/login", {
    method: "POST",
    headers: {
      Authorization: `Basic ${btoa(`${email}:${password}`)}`,
    },
  })
    .then((res) => {
      if (res.status !== 200) {
        return Promise.resolve(null);
      } else {
        return res.json();
      }
    })
    .then((data) => {
      if (data && data.token) {
        localStorage.setItem("authToken", data.token);
        return data.user;
      }
      return null;
    });
};

// Logout: remove the token
export const logout = () => {
  localStorage.removeItem("authToken");
  return Promise.resolve();
};

// Helper to get auth headers for subsequent requests
export const getAuthHeaders = () => {
  const token = localStorage.getItem("authToken");
  return token ? { Authorization: `Bearer ${token}` } : {};
};

// Get logged in user (calls /me)
export const tryGetLoggedInUser = () => {
  // Skip call on login or register pages to avoid loops
  const currentPath = window.location.pathname
    .replace(/\/+$/, "")
    .toLowerCase();
  if (currentPath === "/login" || currentPath === "/register") {
    console.log("Skipping /me call on", currentPath);
    return Promise.resolve(null);
  }

  return fetch(_apiUrl + "/me", {
    headers: {
      ...getAuthHeaders(),
    },
  })
    .then((res) => {
      if (res.status === 401) return null;
      return res.json();
    })
    .catch((error) => {
      console.error("Error fetching user profile:", error);
      return null;
    });
};

// Register: return the JWT and user info upon success
export const register = (userProfile) => {
  userProfile.password = btoa(userProfile.password);
  return fetch(_apiUrl + "/register", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(userProfile),
  })
    .then((res) => {
      if (!res.ok) {
        return res.json().then((error) => {
          throw new Error(error.message || "Registration failed.");
        });
      }
      return res.json();
    })
    .then((data) => {
      if (data && data.token) {
        localStorage.setItem("authToken", data.token);
        return data.user;
      }
      return null;
    });
};
