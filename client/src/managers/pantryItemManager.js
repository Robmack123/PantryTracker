const apiUrl = "/api/pantryitem";

export const getPantryItems = () => {
  return fetch(`${apiUrl}`, {
    method: "GET",
    credentials: "include", // Ensures the user's session cookie is sent with the request
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items.");
    }
    return res.json();
  });
};

export const getPantryItemsByCategory = (categoryIds) => {
  const queryString = categoryIds.map((id) => `categoryIds=${id}`).join("&");
  return fetch(`${apiUrl}/by-category?${queryString}`, {
    method: "GET",
    credentials: "include", // Ensures cookies are sent with the request
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items by category.");
    }
    return res.json();
  });
};

export const addOrUpdatePantryItem = (pantryItem) => {
  return fetch(`${apiUrl}`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(pantryItem),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to add or update pantry item.");
    }
    return res.json();
  });
};
