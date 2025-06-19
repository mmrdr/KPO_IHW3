const API_URL = "http://localhost:8000/api/gateway";

export async function createAccount() {
  const res = await fetch(`${API_URL}/account`, { method: "POST" });
  return res.json();
}

export async function deposit(userId, amount) {
  const res = await fetch(`${API_URL}/account/deposit`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, amount }),
  });
  return res.json();
}

export async function getBalance(userId) {
  const res = await fetch(`${API_URL}/account/${userId}/balance`);
  return res.json();
}

export async function createOrder(orderName, price, accountId) {
  const res = await fetch(`${API_URL}/order`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ orderName, price, accountId }),
  });
  return res.json();
}

export async function getOrders(accountId) {
  const res = await fetch(`${API_URL}/orders/${accountId}`);
  return res.json();
}

export async function getOrderStatus(orderId) {
  const res = await fetch(`${API_URL}/order/${orderId}`);
  return res.json();
}
