import React, { useState } from "react";
import {
  createAccount, deposit, getBalance,
  createOrder, getOrders, getOrderStatus
} from "./api";

function App() {
  const [userId, setUserId] = useState("");
  const [accountId, setAccountId] = useState("");
  const [balance, setBalance] = useState(null);
  const [orderName, setOrderName] = useState("");
  const [orderPrice, setOrderPrice] = useState("");
  const [orders, setOrders] = useState([]);
  const [orderId, setOrderId] = useState("");
  const [orderStatus, setOrderStatus] = useState(null);

  return (
    <div style={{ maxWidth: 600, margin: "auto", padding: 20 }}>
      <h2>Online Shop</h2>

      <h3>1. Создать счет</h3>
      <button onClick={async () => {
        const acc = await createAccount();
        setUserId(acc.userId || acc.UserId);
        setAccountId(acc.userId || acc.UserId);
        alert("Создан счет с UserId: " + (acc.userId || acc.UserId));
      }}>
        Создать счет
      </button>
      <div>
        <input
          placeholder="UserId"
          value={userId}
          onChange={e => setUserId(e.target.value)}
          style={{ width: 300, marginTop: 10 }}
        />
      </div>

      <h3>2. Пополнить счет</h3>
      <input
        type="number"
        placeholder="Сумма"
        min="1"
        style={{ width: 120 }}
        id="depositAmount"
      />
      <button onClick={async () => {
        const amount = parseFloat(document.getElementById("depositAmount").value);
        if (!userId || !amount) return alert("Введите UserId и сумму");
        const res = await deposit(userId, amount);
        alert("Баланс: " + res.balance);
      }}>
        Пополнить
      </button>

      <h3>3. Посмотреть баланс</h3>
      <button onClick={async () => {
        if (!userId) return alert("Введите UserId");
        const res = await getBalance(userId);
        setBalance(res.balance);
      }}>
        Получить баланс
      </button>
      {balance !== null && <div>Баланс: {balance}</div>}

      <h3>4. Создать заказ</h3>
      <input
        placeholder="Название заказа"
        value={orderName}
        onChange={e => setOrderName(e.target.value)}
        style={{ width: 200 }}
      />
      <input
        type="number"
        placeholder="Сумма"
        value={orderPrice}
        onChange={e => setOrderPrice(e.target.value)}
        style={{ width: 100, marginLeft: 10 }}
      />
      <button onClick={async () => {
        if (!orderName || !orderPrice || !userId) return alert("Введите все поля");
        const res = await createOrder(orderName, parseFloat(orderPrice), userId);
        alert("Создан заказ с ID: " + res.orderID);
      }}>
        Создать заказ
      </button>

      <h3>5. Список заказов</h3>
      <button onClick={async () => {
        if (!userId) return alert("Введите UserId");
        const res = await getOrders(userId);
        setOrders(res);
      }}>
        Получить заказы
      </button>
      <ul>
        {orders && orders.map(o =>
          <li key={o.orderID || o.OrderID}>
            {o.orderName || o.OrderName} — {o.price || o.Price} — {o.orderStatus || o.OrderStatus}
            <button style={{ marginLeft: 10 }} onClick={() => setOrderId(o.orderID || o.OrderID)}>
              Статус
            </button>
          </li>
        )}
      </ul>

      <h3>6. Статус заказа</h3>
      <input
        placeholder="OrderId"
        value={orderId}
        onChange={e => setOrderId(e.target.value)}
        style={{ width: 300 }}
      />
      <button onClick={async () => {
        if (!orderId) return alert("Введите OrderId");
        const res = await getOrderStatus(orderId);
        setOrderStatus(res.orderStatus || res.OrderStatus);
      }}>
        Получить статус
      </button>
      {orderStatus && <div>Статус: {orderStatus}</div>}
    </div>
  );
}

export default App;
