const apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/pantryitem";

// Helper function to retrieve the JWT token and build the auth header
const getAuthHeaders = () => {
  const token = localStorage.getItem("authToken");
  return token ? { Authorization: `Bearer ${token}` } : {};
};

export const getRecentActivity = () => {
  return fetch(`${apiUrl}/recent-activity`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch recent activity.");
    }
    return res.json();
  });
};
