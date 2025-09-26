using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;


namespace EvCharging.Infrastructure.Services;


public class OwnerService : IOwnerService
{
    private readonly IRepository<EvOwner> _owners;


    public OwnerService(IRepository<EvOwner> owners) { _owners = owners; }


    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest req)
    {
        var exists = (await _owners.FindAsync(o => o.Nic == req.Nic)).Any();
        if (exists) throw new InvalidOperationException($"Owner {req.Nic} already exists");


        var entity = new EvOwner
        {
        Id = req.Nic,
        Nic = req.Nic,
        FullName = req.FullName,
        Email = req.Email,
        Phone = req.Phone,
        IsActive = true
        };
        await _owners.InsertAsync(entity);
        return new OwnerResponse(entity.Nic, entity.FullName, entity.Email, entity.Phone, entity.IsActive);
    }


    public async Task<OwnerResponse?> GetByNicAsync(string nic)
    {
        var entity = await _owners.GetByIdAsync(nic);
        return entity is null ? null : new OwnerResponse(entity.Nic, entity.FullName, entity.Email, entity.Phone, entity.IsActive);
    }


    public async Task<List<OwnerResponse>> GetAllAsync()
    {
        var items = await _owners.GetAllAsync();
        return items.Select(e => new OwnerResponse(e.Nic, e.FullName, e.Email, e.Phone, e.IsActive)).ToList();
    }


    public async Task UpdateAsync(string nic, UpdateOwnerRequest req)
    {
        var entity = await _owners.GetByIdAsync(nic) ?? throw new KeyNotFoundException("Owner not found");
        entity.FullName = req.FullName;
        entity.Email = req.Email;
        entity.Phone = req.Phone;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _owners.UpdateAsync(entity.Id, entity);
    }
}