const API_BASE_URL = "http://3.147.46.97:5000"; // Replace with your EC2 IP
const _apiUrl = `${API_BASE_URL}/api/household`;

export const getHouseholdUsers = () => {
  return fetch(`${_apiUrl}/members`, {
    method: "GET",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      return Promise.reject(new Error("Failed to fetch household members."));
    }
    return res.json();
  });
};

export const removeUserFromHousehold = (userId) => {
  return fetch(`${_apiUrl}/remove-user/${userId}`, {
    method: "DELETE",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      return Promise.reject(new Error("Failed to remove user from household."));
    }
  });
};

export const joinHousehold = (userId, joinCode) => {
  return fetch(`${_apiUrl}/join`, {
    method: "POST",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ userId, joinCode }),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to join household.");
    }
    return res.json(); // Assuming this returns the updated user
  });
};

export const createHousehold = (userId, name) => {
  console.log("Sending payload:", { name, adminUserId: userId });

  return fetch(`${_apiUrl}/create`, {
    // Updated this line
    method: "POST",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name, adminUserId: userId }),
  }).then((res) => {
    if (!res.ok) {
      console.error("Response error:", res);
      return res.json().then((error) => {
        console.error("Error response body:", error);
        throw new Error(error.message || "Failed to create household.");
      });
    }
    return res.json();
  });
};
