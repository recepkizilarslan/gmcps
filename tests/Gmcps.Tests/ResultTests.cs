using Gmcps.Domain;

namespace Gmcps.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasValue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_HasError()
    {
        var result = Result<int>.Failure("something went wrong");
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<int>.Failure("err");
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = Result<int>.Success(1);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(10);
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.IsSuccess);
        Assert.Equal(20, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var result = Result<int>.Failure("err");
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.IsFailure);
        Assert.Equal("err", mapped.Error);
    }

    [Fact]
    public void Bind_OnSuccess_ChainsResults()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(x =>
            x > 3 ? Result<string>.Success("ok") : Result<string>.Failure("too small"));
        Assert.True(bound.IsSuccess);
        Assert.Equal("ok", bound.Value);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        var result = Result<int>.Failure("err");
        Assert.Equal(99, result.GetValueOrDefault(99));
    }
}
