const _apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/household";

// Helper function to get the Authorization header
const getAuthHeaders = () => {
  const token = localStorage.getItem("authToken");
  return token ? { Authorization: `Bearer ${token}` } : {};
};

export const getHouseholdUsers = () => {
  return fetch(`${_apiUrl}/members?`, {
    method: "GET",
    // With JWT, you typically don't need credentials: "include"
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
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
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  }).then((res) => {
    if (!res.ok) {
      return Promise.reject(new Error("Failed to remove user from household."));
    }
    return res.json();
  });
};

export const joinHousehold = (userId, joinCode) => {
  return fetch(`${_apiUrl}/join`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify({ userId, joinCode }),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to join household.");
    }
    return res.json(); // Assuming this returns the updated user info
  });
};

export const createHousehold = (userId, name) => {
  console.log("Sending payload:", { name, adminUserId: userId });
  return fetch(`${_apiUrl}/create`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
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
