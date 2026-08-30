using Auturge.Stores.Stores;

namespace Auturge.Stores.Tests.TestObjects;

public class UserStore() : Store<User, long>(new InMemoryStore<User, long>());
