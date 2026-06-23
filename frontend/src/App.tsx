import { useMemo, useState } from "react";
import heroImage from "./assets-hero.png";
import { clients, orders, type Client, type Order } from "./data/mockData";

type View = "login" | "dashboard" | "orders" | "clients";

const menuItems: Array<{ id: View; label: string }> = [
  { id: "dashboard", label: "Tablero" },
  { id: "orders", label: "Pedidos" },
  { id: "clients", label: "Clientes" }
];

function statusClass(status: string) {
  return status.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "").replaceAll(" ", "-");
}

function numberValue(value: string) {
  return Number(value.replace(/\./g, "").replace(",", ".")) || 0;
}

function orderType(order: Order) {
  const product = order.details[0]?.product ?? "DTF Textil";
  return product.includes("UV") ? "UV DTF" : "DTF Textil";
}

function orderMeters(order: Order) {
  const total = order.details.reduce((sum, detail) => sum + numberValue(detail.quantity), 0);
  return total ? `${total} m` : "0 m";
}

function Logo() {
  return (
    <div className="brand-logo" aria-label="EvaluSystem">
      <svg className="brand-symbol" viewBox="0 0 64 44" aria-hidden="true" focusable="false">
        <path d="M32 22C23 12 23 5 32 1c9 4 9 11 0 21Z" />
        <path d="M32 22C19 18 13 12 16 4c10-1 16 5 16 18Z" />
        <path d="M32 22C45 18 51 12 48 4c-10-1-16 5-16 18Z" />
        <path d="M32 22C20 28 12 28 7 22c6-7 14-7 25 0Z" />
        <path d="M32 22c12 6 20 6 25 0-6-7-14-7-25 0Z" />
        <path d="M32 22v20" />
      </svg>
      <div className="brand-name">EVALUSYSTEM</div>
      <div className="brand-tagline">Impresion UV DTF y Textil</div>
    </div>
  );
}

function Login({ onLogin }: { onLogin: () => void }) {
  return (
    <main className="login-page">
      <section className="brand-panel" aria-label="Resumen de EvaluSystem">
        <img className="brand-image" src={heroImage} alt="" />
        <Logo />
        <div className="brand-copy">
          <p className="eyebrow">Panel de produccion</p>
          <h1>Pedidos, vendedores e impresion en un solo tablero.</h1>
        </div>
      </section>

      <section className="login-panel" aria-label="Inicio de sesion">
        <div className="form-heading">
          <p className="eyebrow">Bienvenido de vuelta</p>
          <h2>Iniciar sesion</h2>
          <p>Ingresa tus credenciales para continuar.</p>
        </div>

        <form
          className="login-form"
          onSubmit={(event) => {
            event.preventDefault();
            onLogin();
          }}
        >
          <label htmlFor="email">
            Correo institucional
            <input id="email" name="email" type="email" placeholder="ventas@empresa.com" autoComplete="email" />
          </label>

          <label htmlFor="password">
            Contrasena
            <input
              id="password"
              name="password"
              type="password"
              placeholder="Ingresa tu contrasena"
              autoComplete="current-password"
            />
          </label>

          <div className="form-options">
            <label className="remember" htmlFor="remember">
              <input id="remember" name="remember" type="checkbox" />
              Recordarme
            </label>
            <a href="#recuperar">Olvide mi contrasena</a>
          </div>

          <button type="submit">Ingresar</button>
        </form>
      </section>
    </main>
  );
}

function Sidebar({ activeView, onNavigate, onLogout }: { activeView: View; onNavigate: (view: View) => void; onLogout: () => void }) {
  return (
    <aside className="app-sidebar">
      <Logo />
      <nav aria-label="Menu principal">
        {menuItems.map((item) => (
          <button
            className={activeView === item.id ? "active" : ""}
            key={item.id}
            type="button"
            onClick={() => onNavigate(item.id)}
          >
            {item.label}
          </button>
        ))}
        <button type="button">Produccion</button>
        <button type="button">Reportes</button>
      </nav>
      <button className="app-sidebar-logout" type="button" onClick={onLogout}>
        Cerrar sesion
      </button>
    </aside>
  );
}

function Dashboard({ onNewOrder }: { onNewOrder: () => void }) {
  const [search, setSearch] = useState("");
  const filteredOrders = orders.filter((order) => {
    const content = `${order.id} ${order.client} ${order.seller} ${orderType(order)} ${order.status}`.toLowerCase();
    return content.includes(search.toLowerCase());
  });

  return (
    <section className="screen">
      <header className="screen-header">
        <div>
          <p className="eyebrow">Operacion diaria</p>
          <h1>Tablero de impresion</h1>
          <p>Resumen actualizado de ventas, pedidos y cola de produccion.</p>
        </div>
        <button className="primary-button" type="button" onClick={onNewOrder}>
          Nuevo pedido
        </button>
      </header>

      <section className="metric-grid" aria-label="Resumen de pedidos">
        <article className="metric-card metric-blue">
          <span>Pedidos cargados hoy</span>
          <strong>60</strong>
          <small>Total ingresado por ventas</small>
        </article>
        <article className="metric-card metric-green">
          <span>Impresos</span>
          <strong>46</strong>
          <small>Pedidos ya producidos</small>
        </article>
        <article className="metric-card metric-red">
          <span>Faltan imprimir</span>
          <strong>14</strong>
          <small>Pendientes de produccion</small>
        </article>
        <article className="metric-card metric-yellow">
          <span>Entregados</span>
          <strong>50 / 60</strong>
          <small>Pedidos cerrados contra el total</small>
        </article>
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Pedidos recientes</h2>
            <p>Seguimiento desde ventas hasta impresion.</p>
          </div>
          <label className="search-field" htmlFor="order-search">
            Buscar pedido
            <input
              id="order-search"
              type="search"
              placeholder="Vendedor, cliente o tipo"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </label>
        </div>
        <OrdersTable rows={filteredOrders} compact />
      </section>
    </section>
  );
}

function OrdersTable({ rows, compact = false }: { rows: Order[]; compact?: boolean }) {
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Pedido</th>
            {!compact && <th>Fecha carga</th>}
            <th>Cliente</th>
            <th>Vendedor</th>
            <th>Tipo</th>
            <th>Metros</th>
            <th>Estado</th>
            <th>Entrega</th>
            {!compact && <th>Acciones</th>}
          </tr>
        </thead>
        <tbody>
          {rows.map((order) => (
            <tr key={order.id}>
              <td>{order.id}</td>
              {!compact && <td>{order.date}</td>}
              <td>{order.client}</td>
              <td>{order.seller}</td>
              <td>{orderType(order)}</td>
              <td>{orderMeters(order)}</td>
              <td>
                <span className={`status ${statusClass(order.status)}`}>{order.status}</span>
              </td>
              <td>{order.delivery}</td>
              {!compact && (
                <td>
                  <div className="row-actions">
                    <button type="button">Editar</button>
                    <button type="button">Eliminar</button>
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Orders() {
  const [dateFrom, setDateFrom] = useState("2026-06-22");
  const [dateTo, setDateTo] = useState("2026-06-22");
  const [client, setClient] = useState("");
  const [status, setStatus] = useState("");

  const filteredOrders = useMemo(
    () =>
      orders.filter((order) => {
        const matchesFrom = !dateFrom || order.date >= dateFrom;
        const matchesTo = !dateTo || order.date <= dateTo;
        const matchesClient = !client || order.client === client;
        const matchesStatus = !status || order.status === status;
        return matchesFrom && matchesTo && matchesClient && matchesStatus;
      }),
    [client, dateFrom, dateTo, status]
  );

  return (
    <section className="screen">
      <header className="screen-header">
        <div>
          <p className="eyebrow">ABM de pedidos</p>
          <h1>Pedidos</h1>
          <p>Pedidos cargados en el dia, con filtros por fecha y cliente.</p>
        </div>
        <button className="primary-button" type="button">
          Cargar pedido
        </button>
      </header>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Pedidos cargados en el dia</h2>
            <p>{filteredOrders.length} pedidos encontrados</p>
          </div>
        </div>
        <section className="filters" aria-label="Filtros de pedidos">
          <label htmlFor="filter-date-from">
            Fecha desde
            <input id="filter-date-from" type="date" value={dateFrom} onChange={(event) => setDateFrom(event.target.value)} />
          </label>
          <label htmlFor="filter-date-to">
            Fecha hasta
            <input id="filter-date-to" type="date" value={dateTo} onChange={(event) => setDateTo(event.target.value)} />
          </label>
          <label htmlFor="filter-client">
            Cliente
            <select id="filter-client" value={client} onChange={(event) => setClient(event.target.value)}>
              <option value="">Todos</option>
              {[...new Set(orders.map((order) => order.client))].map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label htmlFor="filter-status">
            Estado
            <select id="filter-status" value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">Todos</option>
              {[...new Set(orders.map((order) => order.status))].map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </section>
        <OrdersTable rows={filteredOrders} />
      </section>
    </section>
  );
}

function Clients() {
  const [search, setSearch] = useState("");
  const [city, setCity] = useState("");
  const [status, setStatus] = useState("");

  const filteredClients = useMemo(
    () =>
      clients.filter((client) => {
        const matchesSearch =
          !search ||
          `${client.name} ${client.document} ${client.email}`.toLowerCase().includes(search.toLowerCase());
        const matchesCity = !city || client.city === city;
        const matchesStatus = !status || client.status === status;
        return matchesSearch && matchesCity && matchesStatus;
      }),
    [city, search, status]
  );

  return (
    <section className="screen">
      <header className="screen-header">
        <div>
          <p className="eyebrow">ABM de clientes</p>
          <h1>Clientes</h1>
          <p>Clientes cargados, con filtros por busqueda, ciudad y estado.</p>
        </div>
        <button className="primary-button" type="button">
          Cargar cliente
        </button>
      </header>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Clientes cargados</h2>
            <p>{filteredClients.length} clientes encontrados</p>
          </div>
        </div>
        <section className="filters" aria-label="Filtros de clientes">
          <label htmlFor="filter-search">
            Buscar cliente
            <input
              id="filter-search"
              type="search"
              placeholder="Nombre, documento o correo"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </label>
          <label htmlFor="filter-city">
            Ciudad
            <select id="filter-city" value={city} onChange={(event) => setCity(event.target.value)}>
              <option value="">Todas</option>
              {[...new Set(clients.map((client) => client.city))].map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label htmlFor="filter-client-status">
            Estado
            <select id="filter-client-status" value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">Todos</option>
              <option>Activo</option>
              <option>Inactivo</option>
            </select>
          </label>
        </section>
        <ClientsTable rows={filteredClients} />
      </section>
    </section>
  );
}

function ClientsTable({ rows }: { rows: Client[] }) {
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Cliente</th>
            <th>Documento</th>
            <th>Telefono</th>
            <th>Correo</th>
            <th>Ciudad</th>
            <th>Transportadora</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((client) => (
            <tr key={client.id}>
              <td>{client.name}</td>
              <td>{client.document}</td>
              <td>{client.phone}</td>
              <td>{client.email}</td>
              <td>{client.city}</td>
              <td>
                <span className={`carrier-status ${client.carrier ? "yes" : ""}`}>{client.carrier ? "Si" : "No"}</span>
              </td>
              <td>
                <span className={`client-status ${client.status.toLowerCase()}`}>{client.status}</span>
              </td>
              <td>
                <div className="row-actions">
                  <button type="button">Editar</button>
                  <button className="danger" type="button">Eliminar</button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function App() {
  const [view, setView] = useState<View>("login");

  if (view === "login") {
    return <Login onLogin={() => setView("dashboard")} />;
  }

  return (
    <main className="app-shell">
      <Sidebar activeView={view} onNavigate={setView} onLogout={() => setView("login")} />
      {view === "dashboard" && <Dashboard onNewOrder={() => setView("orders")} />}
      {view === "orders" && <Orders />}
      {view === "clients" && <Clients />}
    </main>
  );
}
