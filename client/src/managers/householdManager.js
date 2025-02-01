const _apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/household";

export const getHouseholdUsers = () => {
  return fetch(`${_apiUrl}/members`, {
    method: "GET",
    credentials: "include",
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
    credentials: "include",
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
    credentials: "include",
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
    method: "POST",
    credentials: "include",
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
