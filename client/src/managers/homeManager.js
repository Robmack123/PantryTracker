const apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/pantryitem";

export const getRecentActivity = () => {
  return fetch(`${apiUrl}/recent-activity`, {
    method: "GET",
    credentials: "include",
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
