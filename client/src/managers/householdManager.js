const _apiUrl = "/api/household";

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
