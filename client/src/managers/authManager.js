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
      return tryGetLoggedInUser();
    }
  });
};

export const logout = () => {
  return fetch(_apiUrl + "/logout");
};

export const tryGetLoggedInUser = () => {
  return fetch(_apiUrl + "/me")
    .then((res) => {
      if (res.status === 401) {
        // Redirect to login page if unauthorized
        window.location.href = "/login"; // or use navigate from react-router
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
    return tryGetLoggedInUser();
  });
};
