const apiUrl = "/api/pantryitem";

export const getRecentActivity = () => {
  return fetch(`${apiUrl}/recent-activity`, {
    method: "GET",
    credentials: "include", // Include cookies for authentication
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch recent activity.");
    }
    return res.json();
  });
};
