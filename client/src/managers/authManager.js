const _apiUrl = "/api/auth";

export const login = (email, password) => {
  return fetch(_apiUrl + "/login", {
    method: "POST",
    credentials: "same-origin",
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
  return fetch(_apiUrl + "/me").then((res) => {
    return res.status === 401 ? Promise.resolve(null) : res.json();
  });
};

export const register = (userProfile) => {
  userProfile.password = btoa(userProfile.password);
  return fetch("/api/auth/register", {
    credentials: "same-origin",
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
