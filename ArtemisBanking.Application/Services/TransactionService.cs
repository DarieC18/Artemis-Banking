using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _cuentas;
        private readonly IBeneficiaryRepository _beneficiarios;
        private readonly ILoanRepository _prestamos;
        private readonly ILoanPaymentScheduleRepository _cuotas;
        private readonly ICreditCardRepository _tarjetas;
        private readonly ITransactionRepository _transacciones;
        private readonly IEmailService _emailService;
        private readonly IUserInfoService _userInfoService;
        private readonly ICreditCardConsumptionRepository _creditCardConsumptions;


        public TransactionService(
            ISavingsAccountRepository cuentas,
            IBeneficiaryRepository beneficiarios,
            ILoanRepository prestamos,
            ILoanPaymentScheduleRepository cuotas,
            ICreditCardRepository tarjetas,
            ITransactionRepository transacciones,
            IEmailService emailService,
            IUserInfoService userInfoService,
            ICreditCardConsumptionRepository creditCardConsumptions)
        {
            _cuentas = cuentas;
            _beneficiarios = beneficiarios;
            _prestamos = prestamos;
            _cuotas = cuotas;
            _tarjetas = tarjetas;
            _transacciones = transacciones;
            _emailService = emailService;
            _userInfoService = userInfoService;
            _creditCardConsumptions = creditCardConsumptions;
        }
        public async Task CreateTransactionExpressAsync(string userId, CreateTransactionExpressDTO dto)
        {
            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto debe ser mayor que cero.");

            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no existe o no pertenece al usuario.");

            if (!origen.IsActive)
                throw new InvalidOperationException("La cuenta de origen está inactiva.");

            var now = DateTime.UtcNow;

            if (origen.Balance < dto.Monto)
            {
                var transRechazada = new Transaction
                {
                    SavingsAccountId = origen.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "DÉBITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = dto.CuentaDestino,
                    Estado = "RECHAZADA",
                    OperatedByUserId = userId
                };

                await _transacciones.AddAsync(transRechazada);

                throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
            }

            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);

            if (destino != null && !destino.IsActive)
                throw new InvalidOperationException("La cuenta de destino está inactiva.");

            origen.Balance -= dto.Monto;
            await _cuentas.UpdateAsync(origen);

            if (destino != null)
            {
                destino.Balance += dto.Monto;
                await _cuentas.UpdateAsync(destino);
            }

            var transDebito = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = dto.CuentaDestino,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(transDebito);

            if (destino != null)
            {
                var transCredito = new Transaction
                {
                    SavingsAccountId = destino.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "CRÉDITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = dto.CuentaDestino,
                    Estado = "APROBADA",
                    OperatedByUserId = userId
                };

                await _transacciones.AddAsync(transCredito);
            }

            var originOwner = await _userInfoService.GetUserBasicInfoByIdAsync(origen.UserId);
            if (!string.IsNullOrWhiteSpace(originOwner?.Email))
            {
                var destLast4 = GetLast4(dto.CuentaDestino);

                var subject = $"Transacción realizada a la cuenta {destLast4}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f7f7f7; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; margin: auto; background: white; border-radius: 10px; padding: 25px; border: 1px solid #e6e6e6;"">
                      <tr>
                        <td>

                          <h2 style=""color: #4a4a4a; margin-bottom: 10px;"">
                            Notificación de transacción
                          </h2>

                          <p>Hola <strong>{originOwner.Nombre} {originOwner.Apellido}</strong>,</p>

                          <p>
                            Se ha realizado una transacción desde tu cuenta
                            <strong>{MaskAccountNumber(dto.CuentaOrigen)}</strong> hacia la cuenta terminada en
                            <strong>{destLast4}</strong>.
                          </p>

                          <p style=""margin-top: 20px;"">
                            <strong>Monto enviado:</strong> {dto.Monto:C}<br/>
                            <strong>Fecha y hora:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <div style=""margin: 25px 0; padding: 15px; background-color: #fff4e5; border-left: 4px solid #ffa726;"">
                            Si no reconoces esta operación, contacta al banco de inmediato.
                          </div>

                          <p style=""color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = originOwner.Email,
                    Subject = subject,
                    Body = body
                });
            }

            if (destino != null)
            {
                var destOwner = await _userInfoService.GetUserBasicInfoByIdAsync(destino.UserId);
                if (!string.IsNullOrWhiteSpace(destOwner?.Email))
                {
                    var originLast4 = GetLast4(dto.CuentaOrigen);

                    var subject = $"Transacción enviada desde la cuenta {originLast4}";

                    var body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; padding: 25px; border: 1px solid #e0e0e0;"">
                          <tr>
                            <td>

                              <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                                Notificación de transacción recibida
                              </h2>

                              <p style=""font-size: 15px; margin-bottom: 10px;"">
                                Hola <strong>{destOwner.Nombre} {destOwner.Apellido}</strong>,
                              </p>

                              <p style=""font-size: 15px; line-height: 1.5;"">
                                Has recibido una transacción desde la cuenta terminada en  
                                <strong>{originLast4}</strong>.
                              </p>

                              <p style=""margin-top: 20px; font-size: 15px;"">
                                <strong>Monto recibido:</strong> {dto.Monto:C}<br/>
                                <strong>Fecha y hora:</strong> {now:dd/MM/yyyy HH:mm}
                              </p>

                              <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                                ArtemisBanking © {DateTime.Now.Year}
                              </p>

                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    ";

                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = destOwner.Email,
                        Subject = subject,
                        Body = body
                    });
                }
            }
        }

        public async Task CreateTransactionToBeneficiaryAsync(string userId, CreateTransactionBeneficiaryDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto a transferir debe ser mayor que cero.");

            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no existe o no pertenece al usuario.");

            if (!origen.IsActive)
                throw new InvalidOperationException("La cuenta de origen está inactiva.");

            var now = DateTime.UtcNow;
            var beneficiario = await _beneficiarios.GetByIdAsync(dto.BeneficiaryId);

            if (beneficiario == null)
                throw new InvalidOperationException("Beneficiario no encontrado.");

            if (string.IsNullOrWhiteSpace(beneficiario.NumeroCuentaBeneficiario))
                throw new InvalidOperationException("El beneficiario no tiene configurada una cuenta de destino.");

            var destino = await _cuentas.GetByAccountNumberAsync(beneficiario.NumeroCuentaBeneficiario);
            if (destino == null)
                throw new InvalidOperationException("La cuenta de destino del beneficiario no existe.");

            if (!destino.IsActive)
                throw new InvalidOperationException("La cuenta de destino del beneficiario está inactiva.");

            if (origen.Balance < dto.Monto)
            {
                var transRechazada = new Transaction
                {
                    SavingsAccountId = origen.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "DÉBITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = beneficiario.NumeroCuentaBeneficiario,
                    Estado = "RECHAZADA",
                    OperatedByUserId = userId
                };
                await _transacciones.AddAsync(transRechazada);
                throw new InvalidOperationException("Fondos insuficientes.");
            }

            origen.Balance -= dto.Monto;
            destino.Balance += dto.Monto;

            await _cuentas.UpdateAsync(origen);
            await _cuentas.UpdateAsync(destino);

            var transDebito = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = beneficiario.NumeroCuentaBeneficiario,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            var transCredito = new Transaction
            {
                SavingsAccountId = destino.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = beneficiario.NumeroCuentaBeneficiario,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(transDebito);
            await _transacciones.AddAsync(transCredito);

            var originOwner = await _userInfoService.GetUserBasicInfoByIdAsync(origen.UserId);

            var beneficiaryUser = !string.IsNullOrWhiteSpace(beneficiario.UserId)
                ? await _userInfoService.GetUserBasicInfoByIdAsync(beneficiario.UserId)
                : null;

            var cuentaDestino = beneficiario.NumeroCuentaBeneficiario;
            var destLast4 = GetLast4(cuentaDestino);
            var originLast4 = GetLast4(dto.CuentaOrigen);

            if (!string.IsNullOrWhiteSpace(originOwner?.Email))
            {
                var subject = $"Transacción realizada a la cuenta {destLast4}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; padding: 25px; border: 1px solid #e0e0e0;"">
                      <tr>
                        <td>

                          <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                            Notificación de transferencia enviada
                          </h2>

                          <p style=""font-size: 15px; margin-bottom: 10px;"">
                            Hola <strong>{originOwner.Nombre} {originOwner.Apellido}</strong>,
                          </p>

                          <p style=""font-size: 15px; line-height: 1.5;"">
                            Se ha realizado una transferencia a un beneficiario desde tu cuenta  
                            <strong>{MaskAccountNumber(dto.CuentaOrigen)}</strong>.
                          </p>

                          <p style=""margin-top: 20px; font-size: 15px;"">
                            <strong>Cuenta destino del beneficiario:</strong> ****{destLast4}<br/>
                            <strong>Monto enviado:</strong> {dto.Monto:C}<br/>
                            <strong>Fecha y hora:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5; border-left: 4px solid #ffa726; font-size: 14px;"">
                            Si no reconoces esta operación, contacta al banco de inmediato.
                          </div>

                          <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = originOwner.Email,
                    Subject = subject,
                    Body = body
                });
            }

            if (beneficiaryUser != null && !string.IsNullOrWhiteSpace(beneficiaryUser.Email))
            {
                var subject = $"Transacción enviada desde la cuenta {originLast4}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; padding: 25px; border: 1px solid #e0e0e0;"">
                      <tr>
                        <td>

                          <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                            Notificación de transferencia recibida
                          </h2>

                          <p style=""font-size: 15px; margin-bottom: 10px;"">
                            Hola <strong>{beneficiaryUser.Nombre} {beneficiaryUser.Apellido}</strong>,
                          </p>

                          <p style=""font-size: 15px; line-height: 1.5;"">
                            Has recibido una transferencia desde la cuenta terminada en  
                            <strong>{originLast4}</strong>.
                          </p>

                          <p style=""margin-top: 20px; font-size: 15px;"">
                            <strong>Monto recibido:</strong> {dto.Monto:C}<br/>
                            <strong>Fecha y hora:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = beneficiaryUser.Email,
                    Subject = subject,
                    Body = body
                });
            }
        }

        public async Task TransferBetweenAccountsAsync(string userId, TransferBetweenAccountsDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);

            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto a transferir debe ser mayor que cero.");

            if (origen == null || origen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no existe o no pertenece al usuario.");

            if (destino == null || destino.UserId != userId)
                throw new InvalidOperationException("La cuenta de destino no existe o no pertenece al usuario.");

            if (origen.Id == destino.Id)
                throw new InvalidOperationException("La cuenta de origen y destino no pueden ser la misma.");

            if (!origen.IsActive)
                throw new InvalidOperationException("La cuenta de origen está inactiva.");

            if (!destino.IsActive)
                throw new InvalidOperationException("La cuenta de destino está inactiva.");

            var now = DateTime.UtcNow;

            if (origen.Balance < dto.Monto)
            {
                var transRechazada = new Transaction
                {
                    SavingsAccountId = origen.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "DÉBITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = dto.CuentaDestino,
                    Estado = "RECHAZADA",
                    OperatedByUserId = userId
                };
                await _transacciones.AddAsync(transRechazada);
                throw new InvalidOperationException("Fondos insuficientes.");
            }

            origen.Balance -= dto.Monto;
            destino.Balance += dto.Monto;

            await _cuentas.UpdateAsync(origen);
            await _cuentas.UpdateAsync(destino);

            var transDebito = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = dto.CuentaDestino,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            var transCredito = new Transaction
            {
                SavingsAccountId = destino.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = dto.CuentaDestino,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(transDebito);
            await _transacciones.AddAsync(transCredito);
        }

        public async Task PayLoanAsync(string userId, PayLoanDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto debe ser mayor que cero.");

            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no existe o no pertenece al usuario.");

            if (!origen.IsActive)
                throw new InvalidOperationException("La cuenta de origen está inactiva.");

            var prestamo = await _prestamos.GetByIdWithScheduleAsync(dto.LoanId);

            if (prestamo == null)
                throw new InvalidOperationException("Préstamo no encontrado.");

            if (prestamo.UserId != userId)
                throw new InvalidOperationException("El préstamo no pertenece al usuario.");

            if (!prestamo.IsActive)
                throw new InvalidOperationException("El préstamo no está activo.");

            var now = DateTime.UtcNow;

            if (origen.Balance < dto.Monto)
            {
                var transRechazada = new Transaction
                {
                    SavingsAccountId = origen.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "DÉBITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = prestamo.NumeroPrestamo,
                    Estado = "RECHAZADA",
                    OperatedByUserId = userId
                };
                await _transacciones.AddAsync(transRechazada);
                throw new InvalidOperationException("Fondos insuficientes.");
            }

            var cuotasPendientes = prestamo.TablaAmortizacion
                .Where(c => c.SaldoPendiente > 0)
                .OrderBy(c => c.FechaPago)
                .ThenBy(c => c.NumeroCuota)
                .ToList();

            if (!cuotasPendientes.Any())
                throw new InvalidOperationException("Este préstamo ya no tiene cuotas pendientes.");

            var montoOriginalIngresado = dto.Monto;
            var montoRestante = dto.Monto;

            origen.Balance -= montoOriginalIngresado;
            await _cuentas.UpdateAsync(origen);

            foreach (var cuota in cuotasPendientes)
            {
                if (montoRestante <= 0)
                    break;

                var montoAplicadoCuota = Math.Min(montoRestante, cuota.SaldoPendiente);

                cuota.SaldoPendiente -= montoAplicadoCuota;
                if (cuota.SaldoPendiente <= 0.01m)
                {
                    cuota.SaldoPendiente = 0;
                    cuota.Pagada = true;
                    cuota.Atrasada = false;
                }

                montoRestante -= montoAplicadoCuota;
            }
            var montoAplicado = montoOriginalIngresado - montoRestante;

            if (montoRestante > 0)
            {
                origen.Balance += montoRestante;
                await _cuentas.UpdateAsync(origen);
            }

            prestamo.CuotasPagadas = prestamo.TablaAmortizacion.Count(c => c.Pagada);
            prestamo.MontoPendiente = prestamo.TablaAmortizacion.Sum(c => c.SaldoPendiente);

            await _prestamos.UpdateAsync(prestamo);

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = montoAplicado,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = prestamo.NumeroPrestamo,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(trans);

            var owner = await _userInfoService.GetUserBasicInfoByIdAsync(origen.UserId);
            var originLast4 = GetLast4(dto.CuentaOrigen);
            var loanNumber = prestamo.NumeroPrestamo;

            if (!string.IsNullOrWhiteSpace(owner?.Email))
            {
                var subject = $"Pago realizado al préstamo {loanNumber}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
                           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; 
                                  padding: 25px; border: 1px solid #e0e0e0;"">
                      <tr>
                        <td>

                          <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                            Notificación de pago de préstamo
                          </h2>

                          <p style=""font-size: 15px; margin-bottom: 10px;"">
                            Hola <strong>{owner.Nombre} {owner.Apellido}</strong>,
                          </p>

                          <p style=""font-size: 15px; line-height: 1.5;"">
                            Se ha realizado un pago a tu préstamo  
                            <strong>{loanNumber}</strong>.
                          </p>

                          <p style=""margin-top: 20px; font-size: 15px;"">
                            <strong>Monto pagado:</strong> {montoAplicado:C}<br/>
                            <strong>Cuenta de débito :</strong> {originLast4}<br/>
                            <strong>Fecha y hora de la transacción:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5; 
                                      border-left: 4px solid #ffa726; font-size: 14px;"">
                            Si no reconoces esta operación, contacta al banco de inmediato.
                          </div>

                          <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = owner.Email,
                    Subject = subject,
                    Body = body
                });
            }
        }

        public async Task PayCreditCardAsync(string userId, PayCreditCardDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto a pagar debe ser mayor que cero.");

            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no existe o no pertenece al usuario.");

            if (!origen.IsActive)
                throw new InvalidOperationException("La cuenta de origen está inactiva.");

            var tarjeta = await _tarjetas.GetByIdAsync(dto.CreditCardId);
            if (tarjeta == null || tarjeta.UserId != userId)
                throw new InvalidOperationException("La tarjeta no existe o no pertenece al usuario.");

            if (!tarjeta.IsActive)
                throw new InvalidOperationException("La tarjeta se encuentra inactiva.");

            var deudaActual = tarjeta.DeudaActual;
            if (deudaActual <= 0)
                throw new InvalidOperationException("La tarjeta no tiene deuda pendiente.");

            var now = DateTime.UtcNow;

            var montoAPagar = Math.Min(dto.Monto, deudaActual);
            if (origen.Balance < dto.Monto)
            {
                var transRechazada = new Transaction
                {
                    SavingsAccountId = origen.Id,
                    Monto = dto.Monto,
                    FechaTransaccion = now,
                    Tipo = "DÉBITO",
                    Origen = dto.CuentaOrigen,
                    Beneficiario = tarjeta.NumeroTarjeta,
                    Estado = "RECHAZADA",
                    OperatedByUserId = userId
                };
                await _transacciones.AddAsync(transRechazada);
                throw new InvalidOperationException("Fondos insuficientes.");
            }


            origen.Balance -= montoAPagar;
            await _cuentas.UpdateAsync(origen);

            tarjeta.DeudaActual -= montoAPagar;
            if (tarjeta.DeudaActual < 0)
                tarjeta.DeudaActual = 0;

            await _tarjetas.UpdateAsync(tarjeta);

            var transDebito = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = montoAPagar,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = dto.CuentaOrigen,
                Beneficiario = tarjeta.NumeroTarjeta,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(transDebito);

            var owner = await _userInfoService.GetUserBasicInfoByIdAsync(origen.UserId);

            var cardLast4 = GetLast4(tarjeta.NumeroTarjeta);
            var originLast4 = GetLast4(dto.CuentaOrigen);

            if (!string.IsNullOrWhiteSpace(owner?.Email))
            {
                var subject = $"Pago realizado a la tarjeta {cardLast4}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
                           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; 
                                  padding: 25px; border: 1px solid #e0e0e0;"">
                      <tr>
                        <td>

                          <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                            Notificación de pago de tarjeta de crédito
                          </h2>

                          <p style=""font-size: 15px; margin-bottom: 10px;"">
                            Hola <strong>{owner.Nombre} {owner.Apellido}</strong>,
                          </p>

                          <p style=""font-size: 15px; line-height: 1.5;"">
                            Se ha realizado un pago a tu tarjeta de crédito terminada en  
                            <strong>{cardLast4}</strong>.
                          </p>

                          <p style=""margin-top: 20px; font-size: 15px;"">
                            <strong>Monto pagado:</strong> {montoAPagar:C}<br/>
                            <strong>Cuenta débito :</strong> {originLast4}<br/>
                            <strong>Fecha y hora de la transacción:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5; 
                                      border-left: 4px solid #ffa726; font-size: 14px;"">
                            Si no reconoces esta operación, contacta al banco de inmediato.
                          </div>

                          <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = owner.Email,
                    Subject = subject,
                    Body = body
                });
            }
        }

        public async Task CreateCashAdvanceAsync(string userId, CashAdvanceDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto del avance debe ser mayor que cero.");

            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);
            if (destino == null || destino.UserId != userId)
                throw new InvalidOperationException("La cuenta de destino no existe o no pertenece al usuario.");

            if (!destino.IsActive)
                throw new InvalidOperationException("La cuenta de destino está inactiva.");

            var tarjeta = await _tarjetas.GetByIdAsync(dto.CreditCardId);
            if (tarjeta == null || tarjeta.UserId != userId)
                throw new InvalidOperationException("Tarjeta no encontrada o no pertenece al usuario.");

            if (!tarjeta.IsActive)
                throw new InvalidOperationException("La tarjeta no está activa.");

            var disponible = tarjeta.LimiteCredito - tarjeta.DeudaActual;
            if (disponible <= 0)
                throw new InvalidOperationException("La tarjeta no tiene crédito disponible para avances.");

            if (dto.Monto > disponible)
                throw new InvalidOperationException("El monto del avance excede el crédito disponible.");

            var interes = Math.Round(dto.Monto * 0.0625m, 2, MidpointRounding.AwayFromZero);
            var totalDeudaNuevo = dto.Monto + interes;

            destino.Balance += dto.Monto;
            tarjeta.DeudaActual += totalDeudaNuevo;

            await _cuentas.UpdateAsync(destino);
            await _tarjetas.UpdateAsync(tarjeta);

            var now = DateTime.UtcNow;

            var transCredito = new Transaction
            {
                SavingsAccountId = destino.Id,
                Monto = dto.Monto,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = $"Tarjeta:{tarjeta.NumeroTarjeta}",
                Beneficiario = dto.CuentaDestino,
                Estado = "APROBADA",
                OperatedByUserId = userId
            };

            await _transacciones.AddAsync(transCredito);

            var consumo = new CreditCardConsumption
            {
                CreditCardId = tarjeta.Id,
                Monto = totalDeudaNuevo,
                FechaConsumo = now,
                Comercio = $"AVANCE EFECTIVO A {GetLast4(dto.CuentaDestino)}",
                Estado = "APROBADO",
                EsAvanceEfectivo = true
            };

            await _creditCardConsumptions.AddAsync(consumo);

            var owner = await _userInfoService.GetUserBasicInfoByIdAsync(destino.UserId);

            var cardLast4 = GetLast4(tarjeta.NumeroTarjeta);
            var accountLast4 = GetLast4(dto.CuentaDestino);

            if (!string.IsNullOrWhiteSpace(owner?.Email))
            {
                var subject = $"Avance de efectivo desde la tarjeta {cardLast4}";

                var body = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
                           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px; 
                                  padding: 25px; border: 1px solid #e0e0e0;"">
                      <tr>
                        <td>

                          <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                            Notificación de avance de efectivo
                          </h2>

                          <p style=""font-size: 15px; margin-bottom: 10px;"">
                            Hola <strong>{owner.Nombre} {owner.Apellido}</strong>,
                          </p>

                          <p style=""font-size: 15px; line-height: 1.5;"">
                            Se ha realizado un avance de efectivo desde tu tarjeta de crédito terminada en  
                            <strong>{cardLast4}</strong>.
                          </p>

                          <p style=""margin-top: 20px; font-size: 15px;"">
                            <strong>Monto del avance:</strong> {dto.Monto:C}<br/>
                            <strong>Cuenta de depósito:</strong> {accountLast4}<br/>
                            <strong>Fecha y hora de la transacción:</strong> {now:dd/MM/yyyy HH:mm}
                          </p>

                          <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5; 
                                      border-left: 4px solid #ffa726; font-size: 14px;"">
                            Si no reconoces esta operación, contacta al banco de inmediato.
                          </div>

                          <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                            ArtemisBanking © {DateTime.Now.Year}
                          </p>

                        </td>
                      </tr>
                    </table>
                  </body>
                </html>
                ";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = owner.Email,
                    Subject = subject,
                    Body = body
                });
            }
        }

        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            var last4 = accountNumber[^4..];
            return $"****{last4}";
        }

        private static string GetLast4(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= 4)
                return value;

            return value[^4..];
        }
    }
}